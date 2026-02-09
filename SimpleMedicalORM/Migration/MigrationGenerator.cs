using System.Reflection;
using System.Text;
using SimpleMedicalORM.Mapping;
using SimpleMedicalORM.Query;
using SimpleMedicalORM.Connection;

namespace SimpleMedicalORM.Migration
{
    public class TableSchema
    {
        public string TableName { get; set; } = string.Empty;
        public List<ColumnSchema> Columns { get; set; } = new();
    }

    public class ColumnSchema
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
    }

    public class MigrationScript
    {
        public string UpScript { get; set; } = string.Empty;
        public string DownScript { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
    }

    public class MigrationGenerator
    {
        private readonly PostgresConnection _connection;

        public MigrationGenerator(PostgresConnection connection)
        {
            _connection = connection;
        }

        public async Task<MigrationScript> GenerateMigrationAsync(List<Type> entityTypes, string description)
        {
            var script = new MigrationScript
            {
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            var upCommands = new List<string>();
            var downCommands = new List<string>();

            foreach (var entityType in entityTypes)
            {
                var tableName = EntityMapper.GetTableName(entityType);

                var currentSchema = await GetTableSchemaFromDatabaseAsync(tableName);

                var desiredSchema = GetTableSchemaFromEntity(entityType);

                if (currentSchema == null)
                {
                    var createSql = QueryBuilder.BuildCreateTableQuery(entityType);
                    upCommands.Add(createSql);
                    downCommands.Add($"DROP TABLE IF EXISTS {tableName}");
                }
                else
                {
                    var alterCommands = GenerateAlterTableCommands(currentSchema, desiredSchema);
                    upCommands.AddRange(alterCommands.UpCommands);
                    downCommands.AddRange(alterCommands.DownCommands);
                }
            }

            script.UpScript = string.Join(";\n\n", upCommands) + ";";
            script.DownScript = string.Join(";\n\n", downCommands.Reverse<string>()) + ";";

            return script;
        }

        private async Task<TableSchema?> GetTableSchemaFromDatabaseAsync(string tableName)
        {
            var sql = @"
                SELECT 
                    column_name,
                    data_type,
                    is_nullable,
                    column_default
                FROM information_schema.columns
                WHERE table_name = @tableName
                ORDER BY ordinal_position";

            try
            {
                var columns = await _connection.ExecuteQueryAsync(sql, reader =>
                {
                    return new ColumnSchema
                    {
                        Name = reader.GetString(0),
                        DataType = reader.GetString(1),
                        IsNullable = reader.GetString(2) == "YES"
                    };
                }, new Npgsql.NpgsqlParameter("@tableName", tableName));

                if (columns.Count == 0)
                    return null;

                var pkSql = @"
                    SELECT column_name
                    FROM information_schema.key_column_usage
                    WHERE table_name = @tableName
                    AND constraint_name LIKE '%_pkey'";

                var primaryKeys = await _connection.ExecuteQueryAsync(pkSql, reader =>
                    reader.GetString(0),
                    new Npgsql.NpgsqlParameter("@tableName", tableName));

                foreach (var column in columns)
                {
                    column.IsPrimaryKey = primaryKeys.Contains(column.Name);
                }

                return new TableSchema
                {
                    TableName = tableName,
                    Columns = columns
                };
            }
            catch
            {
                return null;
            }
        }

        private TableSchema GetTableSchemaFromEntity(Type entityType)
        {
            var tableName = EntityMapper.GetTableName(entityType);
            var schema = new TableSchema { TableName = tableName };

            var properties = EntityMapper.GetColumnProperties(entityType);

            foreach (var property in properties)
            {
                var column = new ColumnSchema
                {
                    Name = EntityMapper.GetColumnName(property),
                    DataType = EntityMapper.GetSqlType(property),
                    IsNullable = EntityMapper.IsNullable(property),
                    IsPrimaryKey = EntityMapper.IsPrimaryKey(property),
                    IsUnique = EntityMapper.IsUnique(property)
                };

                schema.Columns.Add(column);
            }

            return schema;
        }

        private (List<string> UpCommands, List<string> DownCommands) GenerateAlterTableCommands(
            TableSchema current, TableSchema desired)
        {
            var upCommands = new List<string>();
            var downCommands = new List<string>();

            var tableName = desired.TableName;

            foreach (var desiredColumn in desired.Columns)
            {
                var exists = current.Columns.Any(c => c.Name == desiredColumn.Name);

                if (!exists)
                {
                    var nullable = desiredColumn.IsNullable ? "" : " NOT NULL";
                    var unique = desiredColumn.IsUnique ? " UNIQUE" : "";

                    upCommands.Add(
                        $"ALTER TABLE {tableName} ADD COLUMN {desiredColumn.Name} {desiredColumn.DataType}{nullable}{unique}");

                    downCommands.Add(
                        $"ALTER TABLE {tableName} DROP COLUMN {desiredColumn.Name}");
                }
            }

            foreach (var currentColumn in current.Columns)
            {
                var exists = desired.Columns.Any(c => c.Name == currentColumn.Name);

                if (!exists)
                {
                    upCommands.Add(
                        $"ALTER TABLE {tableName} DROP COLUMN {currentColumn.Name}");

                    var nullable = currentColumn.IsNullable ? "" : " NOT NULL";
                    downCommands.Add(
                        $"ALTER TABLE {tableName} ADD COLUMN {currentColumn.Name} {currentColumn.DataType}{nullable}");
                }
            }

            foreach (var desiredColumn in desired.Columns)
            {
                var currentColumn = current.Columns.FirstOrDefault(c => c.Name == desiredColumn.Name);

                if (currentColumn != null)
                {
                    if (!NormalizeDataType(currentColumn.DataType).Equals(
                        NormalizeDataType(desiredColumn.DataType), StringComparison.OrdinalIgnoreCase))
                    {
                        upCommands.Add(
                            $"ALTER TABLE {tableName} ALTER COLUMN {desiredColumn.Name} TYPE {desiredColumn.DataType}");

                        downCommands.Add(
                            $"ALTER TABLE {tableName} ALTER COLUMN {currentColumn.Name} TYPE {currentColumn.DataType}");
                    }

                    if (currentColumn.IsNullable != desiredColumn.IsNullable)
                    {
                        var nullConstraint = desiredColumn.IsNullable ? "DROP NOT NULL" : "SET NOT NULL";
                        upCommands.Add(
                            $"ALTER TABLE {tableName} ALTER COLUMN {desiredColumn.Name} {nullConstraint}");

                        var reverseConstraint = currentColumn.IsNullable ? "DROP NOT NULL" : "SET NOT NULL";
                        downCommands.Add(
                            $"ALTER TABLE {tableName} ALTER COLUMN {currentColumn.Name} {reverseConstraint}");
                    }
                }
            }

            return (upCommands, downCommands);
        }

        private string NormalizeDataType(string dataType)
        {
            dataType = dataType.ToLower();

            if (dataType.Contains("character varying"))
                return "varchar";

            if (dataType.Contains("integer"))
                return "integer";

            if (dataType.Contains("timestamp without time zone"))
                return "timestamp";

            if (dataType.Contains("timestamp with time zone"))
                return "timestamptz";

            return dataType;
        }
    }
}