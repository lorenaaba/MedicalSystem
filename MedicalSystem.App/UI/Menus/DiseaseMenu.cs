using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using MedicalSystem.App.Services;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.App.UI.Menus
{
    public class DiseaseMenu
    {
        private readonly string _connectionString;

        public DiseaseMenu(string connectionString)
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
                Console.WriteLine("║         DISEASE MANAGEMENT                     ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Create New Disease                         ║");
                Console.WriteLine("║ 2. List All Diseases                          ║");
                Console.WriteLine("║ 3. Update Disease                             ║");
                Console.WriteLine("║ 4. Delete Disease                             ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 0. Back                                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.Write("\nSelect option: ");

                var choice = Console.ReadLine();

                var options = new DbContextOptionsBuilder<MedicalDbContext>()
                    .UseNpgsql(_connectionString)
                    .Options;

                using var context = new MedicalDbContext(options);
                var service = new DiseaseService(context);
                var patientService = new PatientService(context);

                switch (choice)
                {
                    case "1":
                        await CreateDisease(service, patientService);
                        break;
                    case "2":
                        await ListDiseases(service);
                        break;
                    case "3":
                        await UpdateDisease(service);
                        break;
                    case "4":
                        await DeleteDisease(service);
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

        private async Task CreateDisease(DiseaseService service, PatientService patientService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       CREATE NEW DISEASE");
            Console.WriteLine("═══════════════════════════════════════\n");

            try
            {
                Console.Write("Patient ID: ");
                if (!int.TryParse(Console.ReadLine(), out int patientId))
                {
                    Console.WriteLine("❌ Invalid Patient ID!");
                    Console.ReadKey();
                    return;
                }

                var patient = await patientService.GetByIdAsync(patientId);
                if (patient == null)
                {
                    Console.WriteLine("❌ Patient not found!");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Patient: {patient.FirstName} {patient.LastName}\n");

                Console.Write("Disease Name: ");
                var name = Console.ReadLine();

                Console.Write("Start Date (yyyy-MM-dd): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
                {
                    Console.WriteLine("❌ Invalid date format!");
                    Console.ReadKey();
                    return;
                }

                Console.Write("End Date (yyyy-MM-dd) [Leave empty if ongoing]: ");
                var endDateInput = Console.ReadLine();
                DateTime? endDate = null;
                if (!string.IsNullOrWhiteSpace(endDateInput))
                {
                    if (DateTime.TryParse(endDateInput, out DateTime parsedEndDate))
                    {
                        endDate = parsedEndDate;
                    }
                }

                var disease = new Disease
                {
                    Name = name ?? "",
                    StartDate = startDate,
                    EndDate = endDate,
                    PatientId = patientId
                };

                await service.CreateAsync(disease);
                Console.WriteLine($"\n✅ Disease created successfully! ID: {disease.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task ListDiseases(DiseaseService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ALL DISEASES");
            Console.WriteLine("═══════════════════════════════════════\n");

            var diseases = await service.GetAllAsync();

            if (diseases.Count == 0)
            {
                Console.WriteLine("No diseases found.");
            }
            else
            {
                foreach (var d in diseases)
                {
                    Console.WriteLine($"ID: {d.Id}");
                    Console.WriteLine($"Name: {d.Name}");
                    Console.WriteLine($"Patient: {d.Patient.FirstName} {d.Patient.LastName}");
                    Console.WriteLine($"Start Date: {d.StartDate:yyyy-MM-dd}");
                    Console.WriteLine($"End Date: {(d.EndDate.HasValue ? d.EndDate.Value.ToString("yyyy-MM-dd") : "Ongoing")}");
                    Console.WriteLine("───────────────────────────────────────");
                }
                Console.WriteLine($"\nTotal: {diseases.Count} diseases");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdateDisease(DiseaseService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         UPDATE DISEASE");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Disease ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var disease = await service.GetByIdAsync(id);
            if (disease == null)
            {
                Console.WriteLine("❌ Disease not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent data:");
            Console.WriteLine($"Name: {disease.Name}");
            Console.WriteLine($"Patient: {disease.Patient.FirstName} {disease.Patient.LastName}");
            Console.WriteLine("(Press Enter to keep current value)\n");

            Console.Write($"Disease Name [{disease.Name}]: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                disease.Name = name;

            Console.Write($"End Date (yyyy-MM-dd) [Current: {(disease.EndDate.HasValue ? disease.EndDate.Value.ToString("yyyy-MM-dd") : "Ongoing")}]: ");
            var endDateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(endDateInput))
            {
                if (DateTime.TryParse(endDateInput, out DateTime endDate))
                {
                    disease.EndDate = endDate;
                }
            }

            await service.UpdateAsync(disease);
            Console.WriteLine("\n✅ Disease updated successfully!");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task DeleteDisease(DiseaseService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         DELETE DISEASE");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Disease ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var disease = await service.GetByIdAsync(id);
            if (disease == null)
            {
                Console.WriteLine("❌ Disease not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nAre you sure you want to delete:");
            Console.WriteLine($"{disease.Name} (Patient: {disease.Patient.FirstName} {disease.Patient.LastName})?");
            Console.Write("\nType 'YES' to confirm: ");

            if (Console.ReadLine()?.ToUpper() == "YES")
            {
                await service.DeleteAsync(disease);
                Console.WriteLine("\n✅ Disease deleted successfully!");
            }
            else
            {
                Console.WriteLine("\n❌ Deletion cancelled.");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}