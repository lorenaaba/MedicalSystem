using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using System.Threading.Tasks;

namespace MedicalSystem.App.Demos
{
    public class MigrationInfoDemo
    {
        private readonly string _connectionString;

        public MigrationInfoDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║      EF CORE - MIGRATION INFO DEMO             ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var options = new DbContextOptionsBuilder<MedicalDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new MedicalDbContext(options);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

            Console.WriteLine(" Migration Status:\n");

            Console.WriteLine($"✅ Applied Migrations ({appliedMigrations.Count()}):");
            foreach (var migration in appliedMigrations)
            {
                Console.WriteLine($"   • {migration}");
            }

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"\n⏳ Pending Migrations ({pendingMigrations.Count()}):");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"   • {migration}");
                }
            }
            else
            {
                Console.WriteLine("\n✅ All migrations are up to date!");
            }

            Console.WriteLine("\n Database Information:");
            Console.WriteLine($"   Can Connect: {await context.Database.CanConnectAsync()}");
            Console.WriteLine($"   Provider: {context.Database.ProviderName}");

            Console.WriteLine("\n\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}