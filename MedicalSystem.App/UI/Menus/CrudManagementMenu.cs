using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.UI.Menus;
using System.Threading.Tasks;

namespace MedicalSystem.App.UI.Menus
{
    public class CrudManagementMenu
    {
        private readonly string _connectionString;

        public CrudManagementMenu(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════╗");
                Console.WriteLine("║         EF CORE - CRUD MANAGEMENT              ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Patient Management                         ║");
                Console.WriteLine("║ 2. Disease Management                         ║");
                Console.WriteLine("║ 3. Medication Management                      ║");
                Console.WriteLine("║ 4. Specialist Exam Management                 ║");
                Console.WriteLine("║ 5. Doctors (View Only)                        ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 0. Back to Main Menu                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.Write("\nSelect option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        var patientMenu = new PatientMenu(_connectionString);
                        await patientMenu.RunAsync();
                        break;
                    case "2":
                        var diseaseMenu = new DiseaseMenu(_connectionString);
                        await diseaseMenu.RunAsync();
                        break;
                    case "3":
                        var medicationMenu = new MedicationMenu(_connectionString);
                        await medicationMenu.RunAsync();
                        break;
                    case "4":
                        var examMenu = new SpecialistExamMenu(_connectionString);
                        await examMenu.RunAsync();
                        break;
                    case "5":
                        await ViewDoctors();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("❌ Invalid option");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task ViewDoctors()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║         DOCTORS (VIEW ONLY)                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var options = new DbContextOptionsBuilder<MedicalDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new MedicalDbContext(options);
            var doctors = await context.Doctors.ToListAsync();

            foreach (var d in doctors)
            {
                Console.WriteLine($"ID: {d.Id}");
                Console.WriteLine($"Name: Dr. {d.FirstName} {d.LastName}");
                Console.WriteLine($"Specialization: {d.Specialization}");
                Console.WriteLine("───────────────────────────────────────");
            }

            Console.WriteLine($"\nTotal: {doctors.Count} doctors");
            Console.WriteLine("\nℹ️  Note: Doctors are seeded at startup and cannot be modified.");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}