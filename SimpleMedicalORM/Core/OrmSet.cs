using System.Linq.Expressions;
using System.Reflection;
using Npgsql;
using SimpleMedicalORM.Attributes;
using SimpleMedicalORM.Connection;
using SimpleMedicalORM.Mapping;
using SimpleMedicalORM.Query;

namespace SimpleMedicalORM.Core
{
    public class OrmSet<T> where T : class, new()
    {
        private readonly PostgresConnection _connection;
        private readonly ChangeTracker _changeTracker;
        private string? _whereClause;  
        private string? _orderByClause;
        private List<string> _includes = new();  

        public OrmSet(PostgresConnection connection, ChangeTracker changeTracker)
        {
            _connection = connection;
            _changeTracker = changeTracker;

        }

        public OrmSet<T> Include(string navigationPropertyName)
        {
            _includes.Add(navigationPropertyName);
            return this;
        }

        public async Task<List<T>> ToListAsync()
        {
            var sql = QueryBuilder.BuildSelectQuery(typeof(T), _whereClause, _orderByClause);
            var results = await ExecuteQueryAsync(sql);

            foreach (var include in _includes)
            {
                await LoadNavigationPropertyAsync(results, include);
            }

            _whereClause = null;
            _orderByClause = null;
            _includes.Clear();

            return results;
        }

        private async Task LoadNavigationPropertyAsync(List<T> entities, string navigationPropertyName)
        {
            var entityType = typeof(T);
            var navProperty = entityType.GetProperty(navigationPropertyName);

            if (navProperty == null)
                return;

            var navAttr = navProperty.GetCustomAttribute<NavigationPropertyAttribute>();
            if (navAttr == null)
                return;

            var foreignKeyPropertyName = navAttr.ForeignKeyProperty;
            var foreignKeyProperty = entityType.GetProperty(foreignKeyPropertyName);

            if (foreignKeyProperty == null)
                return;

            var relatedType = navProperty.PropertyType;

            if (relatedType.IsGenericType && relatedType.GetGenericTypeDefinition() == typeof(List<>))
            {
                relatedType = relatedType.GetGenericArguments()[0];
            }

            var setType = typeof(OrmSet<>).MakeGenericType(relatedType);
            var set = Activator.CreateInstance(setType, _connection, _changeTracker);

            foreach (var entity in entities)
            {
                var foreignKeyValue = foreignKeyProperty.GetValue(entity);

                if (foreignKeyValue == null)
                    continue;

                var pkProperty = EntityMapper.GetPrimaryKeyProperty(entityType);
                var pkValue = pkProperty?.GetValue(entity);

                var relatedPkProperty = EntityMapper.GetPrimaryKeyProperty(relatedType);
                var relatedFkColumn = EntityMapper.GetColumnName(
                    relatedType.GetProperty(foreignKeyPropertyName) ?? relatedPkProperty!
                );

                var whereClause = $"{relatedFkColumn} = {pkValue}";
                var sql = QueryBuilder.BuildSelectQuery(relatedType, whereClause);

                var relatedData = await _connection.ExecuteQueryAsync(sql, reader =>
                {
                    var relatedEntity = Activator.CreateInstance(relatedType);
                    MapFromReaderGeneric(reader, relatedEntity!, relatedType);
                    return relatedEntity;
                });

                if (navProperty.PropertyType.IsGenericType)
                {
                    var list = Activator.CreateInstance(navProperty.PropertyType);
                    var addMethod = navProperty.PropertyType.GetMethod("Add");

                    foreach (var item in relatedData)
                    {
                        addMethod?.Invoke(list, new[] { item });
                    }

                    navProperty.SetValue(entity, list);
                }
                else
                {
                    navProperty.SetValue(entity, relatedData.FirstOrDefault());
                }
            }
        }

        private void MapFromReaderGeneric(NpgsqlDataReader reader, object entity, Type entityType)
        {
            var properties = EntityMapper.GetColumnProperties(entityType);

            foreach (var property in properties)
            {
                var columnName = EntityMapper.GetColumnName(property);

                try
                {
                    var ordinal = reader.GetOrdinal(columnName);
                    var value = reader.GetValue(ordinal);

                    if (value != DBNull.Value)
                    {
                        var convertedValue = SqlTypeMapper.ConvertFromDb(value, property.PropertyType);
                        property.SetValue(entity, convertedValue);
                    }
                }
                catch
                {
                }
            }
        }

        public OrmSet<T> Where(Expression<Func<T, bool>> predicate)
        {
            _whereClause = ExpressionParser.ParseWhere(predicate);
            return this; 
        }

        public async Task<T?> FindAsync(object primaryKey)
        {
            var pkProperty = EntityMapper.GetPrimaryKeyProperty(typeof(T));
            if (pkProperty == null)
                throw new InvalidOperationException("Entity must have a primary key");

            var pkColumn = EntityMapper.GetColumnName(pkProperty);
            var whereClause = $"{pkColumn} = {primaryKey}";

            var sql = QueryBuilder.BuildSelectQuery(typeof(T), whereClause);
            var results = await ExecuteQueryAsync(sql);

            var entity = results.FirstOrDefault();

            if (entity != null)
            {
                _changeTracker.Track(entity, EntityState.Unchanged);
            }

            return entity;
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var whereClause = ExpressionParser.ParseWhere(predicate);
            var sql = QueryBuilder.BuildSelectQuery(typeof(T), whereClause) + " LIMIT 1";

            var results = await ExecuteQueryAsync(sql);
            var entity = results.FirstOrDefault();

            if (entity != null)
            {
                _changeTracker.Track(entity, EntityState.Unchanged);
            }

            return entity;
        }

        public void Add(T entity)
        {
            _changeTracker.Track(entity, EntityState.Added);
        }

        public void Update(T entity)
        {
            _changeTracker.SetState(entity, EntityState.Modified);
        }

        public void Remove(T entity)
        {
            _changeTracker.SetState(entity, EntityState.Deleted);
        }

        public async Task<T> InsertAsync(T entity)
        {
            var sql = QueryBuilder.BuildInsertQuery(typeof(T), entity);
            var properties = EntityMapper.GetColumnProperties(typeof(T))
                .Where(p => !EntityMapper.IsAutoIncrement(p))
                .ToArray();

            var parameters = properties.Select((p, i) => new NpgsqlParameter($"@param{i}", p.GetValue(entity) ?? DBNull.Value))
                .ToArray();

            var reader = await _connection.ExecuteReaderAsync(sql, parameters);

            if (await reader.ReadAsync())
            {
                MapFromReader(reader, entity);
            }

            await reader.CloseAsync();

            _changeTracker.Track(entity, EntityState.Unchanged);
            return entity;
        }

        public async Task<int> UpdateAsync(T entity)
        {
            var sql = QueryBuilder.BuildUpdateQuery(typeof(T), entity);

            var pkProperty = EntityMapper.GetPrimaryKeyProperty(typeof(T));
            if (pkProperty == null)
                throw new InvalidOperationException("Entity must have a primary key");

            var properties = EntityMapper.GetColumnProperties(typeof(T))
                .Where(p => !EntityMapper.IsPrimaryKey(p))
                .ToArray();

            var parameters = properties.Select((p, i) =>
                new NpgsqlParameter($"@param{i}", p.GetValue(entity) ?? DBNull.Value))
                .ToList();

            var pkParamIndex = properties.Length;
            parameters.Add(new NpgsqlParameter($"@param{pkParamIndex}", pkProperty.GetValue(entity) ?? DBNull.Value));

            var result = await _connection.ExecuteNonQueryAsync(sql, parameters.ToArray());

            if (result > 0)
            {
                _changeTracker.Track(entity, EntityState.Unchanged);
            }

            return result;
        }

        public async Task<int> DeleteAsync(T entity)
        {
            var pkProperty = EntityMapper.GetPrimaryKeyProperty(typeof(T));
            if (pkProperty == null)
                throw new InvalidOperationException("Entity must have a primary key");

            var primaryKey = pkProperty.GetValue(entity);
            var sql = QueryBuilder.BuildDeleteQuery(typeof(T), primaryKey!);

            var result = await _connection.ExecuteNonQueryAsync(sql);

            if (result > 0)
            {
                _changeTracker.SetState(entity, EntityState.Unchanged);
            }

            return result;
        }

        private async Task<List<T>> ExecuteQueryAsync(string sql)
        {
            return await _connection.ExecuteQueryAsync(sql, reader =>
            {
                var entity = new T();
                MapFromReader(reader, entity);
                return entity;
            });
        }

        private void MapFromReader(NpgsqlDataReader reader, T entity)
        {
            var properties = EntityMapper.GetColumnProperties(typeof(T));

            foreach (var property in properties)
            {
                var columnName = EntityMapper.GetColumnName(property);

                try
                {
                    var ordinal = reader.GetOrdinal(columnName);
                    var value = reader.GetValue(ordinal);

                    if (value != DBNull.Value)
                    {
                        var convertedValue = SqlTypeMapper.ConvertFromDb(value, property.PropertyType);
                        property.SetValue(entity, convertedValue);
                    }
                }
                catch
                {
                    
                }
            }
        }

        public OrmSet<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector.Body is MemberExpression memberExpr)
            {
                var propertyInfo = memberExpr.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var columnName = EntityMapper.GetColumnName(propertyInfo);
                    _orderByClause = $"{columnName} ASC";
                }
            }
            return this;
        }

        public OrmSet<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector.Body is MemberExpression memberExpr)
            {
                var propertyInfo = memberExpr.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var columnName = EntityMapper.GetColumnName(propertyInfo);
                    _orderByClause = $"{columnName} DESC";
                }
            }
            return this;
        }
    }
}