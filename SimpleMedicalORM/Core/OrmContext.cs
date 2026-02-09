using SimpleMedicalORM.Connection;
using SimpleMedicalORM.Query;

namespace SimpleMedicalORM.Core
{
    public abstract class OrmContext : IDisposable
    {
        private readonly PostgresConnection _connection;
        private readonly ChangeTracker _changeTracker;
        private readonly Dictionary<Type, object> _sets = new();

        protected OrmContext(string connectionString)
        {
            _connection = new PostgresConnection(connectionString);
            _changeTracker = new ChangeTracker();

            InitializeSets();
        }

        protected virtual void InitializeSets()
        {
        }

        public OrmSet<T> Set<T>() where T : class, new()
        {
            var type = typeof(T);

            if (!_sets.ContainsKey(type))
            {
                _sets[type] = new OrmSet<T>(_connection, _changeTracker);
            }

            return (OrmSet<T>)_sets[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            _changeTracker.DetectChanges();
            var changesCount = 0;

            try
            {
                await _connection.BeginTransactionAsync();

                foreach (var entry in _changeTracker.GetEntries(EntityState.Added).ToList())
                {
                    var set = GetOrCreateSet(entry.Entity.GetType());
                    var method = set.GetType().GetMethod("InsertAsync")!;

                    await (Task)method.Invoke(set, new[] { entry.Entity })!;
                    _changeTracker.AcceptChanges(entry.Entity);
                    changesCount++;
                }

                foreach (var entry in _changeTracker.GetEntries(EntityState.Modified).ToList())
                {
                    var set = GetOrCreateSet(entry.Entity.GetType());
                    var method = set.GetType().GetMethod("UpdateAsync")!;

                    var result = await (Task<int>)method.Invoke(set, new[] { entry.Entity })!;
                    _changeTracker.AcceptChanges(entry.Entity);
                    changesCount += result;
                }

                foreach (var entry in _changeTracker.GetEntries(EntityState.Deleted).ToList())
                {
                    var set = GetOrCreateSet(entry.Entity.GetType());
                    var method = set.GetType().GetMethod("DeleteAsync")!;

                    var result = await (Task<int>)method.Invoke(set, new[] { entry.Entity })!;
                    _changeTracker.Detach(entry.Entity);
                    changesCount += result;
                }

                await _connection.CommitAsync();
                return changesCount;
            }
            catch
            {
                await _connection.RollbackAsync();
                throw;
            }
        }

        private object GetOrCreateSet(Type entityType)
        {
            if (!_sets.ContainsKey(entityType))
            {
                _sets[entityType] = Activator.CreateInstance(
                    typeof(OrmSet<>).MakeGenericType(entityType),
                    _connection,
                    _changeTracker
                )!;
            }

            return _sets[entityType];
        }


        public async Task EnsureCreatedAsync()
        {
            Console.WriteLine("📋 Creating database tables...\n");

            foreach (var kvp in _sets)
            {
                var entityType = kvp.Key;
                var tableName = SimpleMedicalORM.Mapping.EntityMapper.GetTableName(entityType);

                try
                {
                    var sql = QueryBuilder.BuildCreateTableQuery(entityType);
                    Console.WriteLine($"Creating table: {tableName}");
                    await _connection.ExecuteNonQueryAsync(sql);
                    Console.WriteLine($"✅ Table {tableName} created");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("already exists") || ex.Message.Contains("42P07"))
                    {
                        Console.WriteLine($"⚠️  Table {tableName} already exists");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Error creating {tableName}: {ex.Message}");
                        throw;
                    }
                }
            }

            Console.WriteLine();
        }

        public ChangeTracker ChangeTracker => _changeTracker;

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}