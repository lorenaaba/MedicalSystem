using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using SimpleMedicalORM.Connection;

namespace SimpleMedicalORM.Migration
{
    public class MigrationRunner
    {
        private readonly PostgresConnection _connection;
        private const string MigrationHistoryTable = "__CustomOrmMigrationsHistory";

        public MigrationRunner(PostgresConnection connection)
        {
            _connection = connection;
        }

        public async Task EnsureMigrationHistoryTableAsync()
        {
            var sql = $@"
                CREATE TABLE IF NOT EXISTS {MigrationHistoryTable} (
                    migration_id VARCHAR(255) PRIMARY KEY,
                    description VARCHAR(500),
                    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                )";

            try
            {
                await _connection.ExecuteNonQueryAsync(sql);
                Console.WriteLine($"✅ Migration history table '{MigrationHistoryTable}' ready");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating migration history table: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetAppliedMigrationsAsync()
        {
            await EnsureMigrationHistoryTableAsync();

            var sql = $"SELECT migration_id FROM {MigrationHistoryTable} ORDER BY applied_at";

            try
            {
                return await _connection.ExecuteQueryAsync(sql, reader => reader.GetString(0));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error reading migration history: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task RunMigrationAsync(MigrationScript migration, string migrationId)
        {
            await EnsureMigrationHistoryTableAsync();

            var appliedMigrations = await GetAppliedMigrationsAsync();
            if (appliedMigrations.Contains(migrationId))
            {
                Console.WriteLine($"⚠️  Migration '{migrationId}' already applied - skipping");
                return;
            }

            Console.WriteLine($"\n Applying migration: {migrationId}");
            Console.WriteLine($"   Description: {migration.Description}");

            try
            {
                await _connection.BeginTransactionAsync();

                var commands = SplitSqlCommands(migration.UpScript);

                foreach (var command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        Console.WriteLine($"   Executing: {TruncateForDisplay(command)}");
                        await _connection.ExecuteNonQueryAsync(command.Trim());
                    }
                }

                var insertSql = $@"
                    INSERT INTO {MigrationHistoryTable} (migration_id, description, applied_at)
                    VALUES (@migrationId, @description, @appliedAt)";

                await _connection.ExecuteNonQueryAsync(insertSql,
                    new NpgsqlParameter("@migrationId", migrationId),
                    new NpgsqlParameter("@description", migration.Description ?? string.Empty),
                    new NpgsqlParameter("@appliedAt", DateTime.UtcNow)
                );

                await _connection.CommitAsync();

                Console.WriteLine($"✅ Migration '{migrationId}' applied successfully at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                await _connection.RollbackAsync();
                Console.WriteLine($"❌ Migration '{migrationId}' failed: {ex.Message}");
                Console.WriteLine($"   Transaction rolled back");
                throw;
            }
        }

        public async Task RollbackMigrationAsync(MigrationScript migration, string migrationId)
        {
            await EnsureMigrationHistoryTableAsync();

            var appliedMigrations = await GetAppliedMigrationsAsync();
            if (!appliedMigrations.Contains(migrationId))
            {
                Console.WriteLine($"⚠️  Migration '{migrationId}' is not applied - cannot rollback");
                return;
            }

            Console.WriteLine($"\n Rolling back migration: {migrationId}");

            try
            {
                await _connection.BeginTransactionAsync();

                var commands = SplitSqlCommands(migration.DownScript);

                foreach (var command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        Console.WriteLine($"   Executing: {TruncateForDisplay(command)}");
                        await _connection.ExecuteNonQueryAsync(command.Trim());
                    }
                }

                var deleteSql = $"DELETE FROM {MigrationHistoryTable} WHERE migration_id = @migrationId";
                await _connection.ExecuteNonQueryAsync(deleteSql,
                    new NpgsqlParameter("@migrationId", migrationId)
                );

                await _connection.CommitAsync();

                Console.WriteLine($"✅ Migration '{migrationId}' rolled back successfully");
            }
            catch (Exception ex)
            {
                await _connection.RollbackAsync();
                Console.WriteLine($"❌ Rollback of migration '{migrationId}' failed: {ex.Message}");
                Console.WriteLine($"   Transaction rolled back");
                throw;
            }
        }

        public async Task RollbackToMigrationAsync(string? targetMigrationId, Dictionary<string, MigrationScript> allMigrations)
        {
            var appliedMigrations = await GetAppliedMigrationsAsync();

            if (appliedMigrations.Count == 0)
            {
                Console.WriteLine("  No migrations to rollback");
                return;
            }

            var migrationsToRollback = targetMigrationId == null
                ? appliedMigrations
                : appliedMigrations.SkipWhile(m => m != targetMigrationId).Skip(1).ToList();

            if (migrationsToRollback.Count == 0)
            {
                Console.WriteLine($"  Already at migration '{targetMigrationId}'");
                return;
            }

            Console.WriteLine($"\n Rolling back {migrationsToRollback.Count} migration(s)...");

            foreach (var migrationId in migrationsToRollback.Reverse<string>())
            {
                if (allMigrations.TryGetValue(migrationId, out var migration))
                {
                    await RollbackMigrationAsync(migration, migrationId);
                }
                else
                {
                    Console.WriteLine($"⚠️  Migration script for '{migrationId}' not found - skipping");
                }
            }

            Console.WriteLine($"\n✅ Rollback completed");
        }

        public async Task ShowMigrationStatusAsync(List<string> availableMigrationIds)
        {
            var appliedMigrations = await GetAppliedMigrationsAsync();

            Console.WriteLine("\n Migration Status:");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            if (appliedMigrations.Count > 0)
            {
                Console.WriteLine("\n✅ Applied Migrations:");
                foreach (var migration in appliedMigrations)
                {
                    Console.WriteLine($"   • {migration}");
                }
            }

            var pendingMigrations = availableMigrationIds.Except(appliedMigrations).ToList();
            if (pendingMigrations.Count > 0)
            {
                Console.WriteLine("\n Pending Migrations:");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"   • {migration}");
                }
            }

            if (appliedMigrations.Count == 0 && pendingMigrations.Count == 0)
            {
                Console.WriteLine("   No migrations found");
            }

            Console.WriteLine("═══════════════════════════════════════════════════════════\n");
        }

        private List<string> SplitSqlCommands(string sql)
        {
            return sql
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(cmd => cmd.Trim())
                .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                .ToList();
        }

        private string TruncateForDisplay(string sql, int maxLength = 80)
        {
            var cleaned = sql.Replace("\n", " ").Replace("\r", " ").Trim();

            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }

            if (cleaned.Length <= maxLength)
                return cleaned;

            return cleaned.Substring(0, maxLength - 3) + "...";
        }
    }
}