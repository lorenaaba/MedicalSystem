using System.Reflection;
using System.Text;
using SimpleMedicalORM.Mapping;

namespace SimpleMedicalORM.Query
{
    public class QueryBuilder
    {
        public static string BuildSelectQuery(
            Type entityType,
            string? whereClause = null,
            string? orderByClause = null)  
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var properties = EntityMapper.GetColumnProperties(entityType);
            var columnNames = properties.Select(EntityMapper.GetColumnName);
            var columns = string.Join(", ", columnNames);

            var sql = $"SELECT {columns} FROM {tableName}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += $" WHERE {whereClause}";
            }

            if (!string.IsNullOrEmpty(orderByClause)) 
            {
                sql += $" ORDER BY {orderByClause}";
            }

            return sql;
        }

        public static string BuildInsertQuery(Type entityType, object entity)
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var properties = EntityMapper.GetColumnProperties(entityType)
                .Where(p => !EntityMapper.IsAutoIncrement(p))
                .ToList();

            var columns = properties.Select(p => EntityMapper.GetColumnName(p));
            var values = properties.Select((p, i) => $"@param{i}");

            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) " +
                   $"VALUES ({string.Join(", ", values)}) RETURNING *";
        }

        public static string BuildUpdateQuery(Type entityType, object entity)
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var pkProperty = EntityMapper.GetPrimaryKeyProperty(entityType);

            if (pkProperty == null)
                throw new InvalidOperationException("Entity must have a primary key");

            var properties = EntityMapper.GetColumnProperties(entityType)
                .Where(p => !EntityMapper.IsPrimaryKey(p))
                .ToList();

            var setClauses = properties.Select((p, i) =>
                $"{EntityMapper.GetColumnName(p)} = @param{i}");

            var pkColumn = EntityMapper.GetColumnName(pkProperty);
            var pkParamIndex = properties.Count; 

            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} " +
                   $"WHERE {pkColumn} = @param{pkParamIndex}"; 
        }

        public static string BuildDeleteQuery(Type entityType, object primaryKeyValue)
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var pkProperty = EntityMapper.GetPrimaryKeyProperty(entityType);

            if (pkProperty == null)
                throw new InvalidOperationException("Entity must have a primary key");

            var pkColumn = EntityMapper.GetColumnName(pkProperty);

            return $"DELETE FROM {tableName} WHERE {pkColumn} = {primaryKeyValue}";
        }

        public static string BuildCreateTableQuery(Type entityType)
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var properties = EntityMapper.GetColumnProperties(entityType);

            var columnDefinitions = new List<string>();
            var constraints = new List<string>();

            foreach (var property in properties)
            {
                var columnName = EntityMapper.GetColumnName(property);
                var sqlType = EntityMapper.GetSqlType(property);
                var nullable = EntityMapper.IsNullable(property) ? "" : " NOT NULL";

                var columnDef = $"{columnName} {sqlType}{nullable}";

                if (EntityMapper.IsPrimaryKey(property))
                {
                    if (EntityMapper.IsAutoIncrement(property))
                    {
                        columnDef = $"{columnName} SERIAL PRIMARY KEY";
                    }
                    else
                    {
                        columnDef += " PRIMARY KEY";
                    }
                }
                else if (EntityMapper.IsUnique(property))
                {
                    columnDef += " UNIQUE";
                }

                columnDefinitions.Add(columnDef);
            }

            return $"CREATE TABLE IF NOT EXISTS {tableName} (\n  " +
                   $"{string.Join(",\n  ", columnDefinitions)}\n)";
        }
    }
}