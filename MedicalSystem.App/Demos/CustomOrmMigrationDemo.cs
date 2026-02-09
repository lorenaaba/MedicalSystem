using System.Diagnostics;
using SimpleMedicalORM.Connection;
using SimpleMedicalORM.Migration;

namespace MedicalSystem.App.Demos
{
    public class CustomOrmMigrationDemo
    {
        private readonly string _connectionString;

        public CustomOrmMigrationDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║    CUSTOM ORM - MIGRATION SYSTEM DEMO          ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var connection = new PostgresConnection(_connectionString);
            var runner = new MigrationRunner(connection);

            try
            {
                await runner.EnsureMigrationHistoryTableAsync();

                Console.WriteLine(" Step 1: Current Migration Status\n");
                var testMigrations = new List<string>
                {
                    "20250209_CreatePatients",
                    "20250209_CreateDoctors",
                    "20250209_CreateDiseases"
                };
                await runner.ShowMigrationStatusAsync(testMigrations);

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();

                Console.WriteLine("\n\n Step 2: Creating a test table migration...\n");

                var createTableMigration = new MigrationScript
                {
                    UpScript = @"
                        CREATE TABLE IF NOT EXISTS test_migration_table (
                            id SERIAL PRIMARY KEY,
                            name VARCHAR(100) NOT NULL,
                            created_at TIMESTAMP DEFAULT NOW()
                        );
                    ",
                    DownScript = @"
                        DROP TABLE IF EXISTS test_migration_table;
                    "
                };

                Console.WriteLine("UP Script:");
                Console.WriteLine(createTableMigration.UpScript);
                Console.WriteLine("\nDOWN Script:");
                Console.WriteLine(createTableMigration.DownScript);

                Console.WriteLine("\n\nPress any key to apply migration...");
                Console.ReadKey();

                Console.WriteLine("\n Step 3: Applying migration...\n");
                await runner.RunMigrationAsync(createTableMigration, "20250209_TestTable");

                Console.WriteLine("\nPress any key to check status...");
                Console.ReadKey();

                Console.WriteLine("\n\n Step 4: Migration Status After Apply\n");
                var updatedMigrations = new List<string>
                {
                    "20250209_CreatePatients",
                    "20250209_CreateDoctors",
                    "20250209_CreateDiseases",
                    "20250209_TestTable"
                };
                await runner.ShowMigrationStatusAsync(updatedMigrations);

                Console.WriteLine("\n\n Step 5: Rollback Demo");
                Console.WriteLine("Do you want to rollback the test migration? (y/n): ");
                var choice = Console.ReadLine();

                if (choice?.ToLower() == "y")
                {
                    Console.WriteLine("\n Rolling back migration...\n");
                    await runner.RollbackMigrationAsync(createTableMigration, "20250209_TestTable");

                    Console.WriteLine("\n Final Status:\n");
                    await runner.ShowMigrationStatusAsync(updatedMigrations);
                }

                Console.WriteLine("\n✅ Migration demo completed!");
                Console.WriteLine("\n  This demonstrates:");
                Console.WriteLine("   • Migration history tracking");
                Console.WriteLine("   • UP/DOWN script execution");
                Console.WriteLine("   • Rollback functionality");
                Console.WriteLine("   • Migration status reporting");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error in migration demo: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                connection.Dispose();
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}