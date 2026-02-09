using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Services; 
using MedicalSystem.App.UI;

namespace MedicalSystem.App
{
    class Program
    {
        private static readonly string _connectionString =
            "Host=localhost;Port=5432;Database=medicaldb;Username=medicaluser;Password=medicalpass123";

        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║        MEDICAL SYSTEM - UNIFIED APP            ║");
            Console.WriteLine("║     EF Core + Custom ORM Demonstration         ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            await InitializeDatabaseAsync();

            var menuSystem = new MenuSystem(_connectionString);
            await menuSystem.RunAsync();
        }

        private static async Task InitializeDatabaseAsync()
        {
            var options = new DbContextOptionsBuilder<MedicalDbContext>()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;

            using var context = new MedicalDbContext(options);

            Console.WriteLine("🔌 Connecting to database...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Database migrated\n");

            var doctorService = new DoctorService(context);
            await doctorService.SeedInitialDoctorsAsync();
        }
    }
}