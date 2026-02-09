using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using MedicalSystem.App.Services;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.App.UI.Menus
{
    public class MedicationMenu
    {
        private readonly string _connectionString;

        public MedicationMenu(string connectionString)
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
                Console.WriteLine("║         MEDICATION MANAGEMENT                  ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Create New Medication                      ║");
                Console.WriteLine("║ 2. List All Medications                       ║");
                Console.WriteLine("║ 3. Update Medication                          ║");
                Console.WriteLine("║ 4. Delete Medication                          ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 0. Back                                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.Write("\nSelect option: ");

                var choice = Console.ReadLine();

                var options = new DbContextOptionsBuilder<MedicalDbContext>()
                    .UseNpgsql(_connectionString)
                    .Options;

                using var context = new MedicalDbContext(options);
                var service = new MedicationService(context);
                var patientService = new PatientService(context);

                switch (choice)
                {
                    case "1":
                        await CreateMedication(service, patientService);
                        break;
                    case "2":
                        await ListMedications(service);
                        break;
                    case "3":
                        await UpdateMedication(service);
                        break;
                    case "4":
                        await DeleteMedication(service);
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

        private async Task CreateMedication(MedicationService service, PatientService patientService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       CREATE NEW MEDICATION");
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

                Console.Write("Medication Name: ");
                var name = Console.ReadLine();

                Console.Write("Dosage (e.g. 10mg, 2 tablets): ");
                var dosage = Console.ReadLine();

                Console.Write("Frequency (e.g. 2x daily, every 8 hours): ");
                var frequency = Console.ReadLine();

                Console.Write("Notes [Optional]: ");
                var notes = Console.ReadLine();

                Console.Write("Disease ID [Optional, leave empty if not linked]: ");
                int? diseaseId = null;
                var diseaseInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(diseaseInput) && int.TryParse(diseaseInput, out int dId))
                {
                    diseaseId = dId;
                }

                var medication = new Medication
                {
                    Name = name ?? "",
                    Dosage = dosage ?? "",
                    Frequency = frequency ?? "",
                    Notes = notes,
                    PatientId = patientId,
                    DiseaseId = diseaseId
                };

                await service.CreateAsync(medication);
                Console.WriteLine($"\n✅ Medication created successfully! ID: {medication.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task ListMedications(MedicationService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ALL MEDICATIONS");
            Console.WriteLine("═══════════════════════════════════════\n");

            var medications = await service.GetAllAsync();

            if (medications.Count == 0)
            {
                Console.WriteLine("No medications found.");
            }
            else
            {
                foreach (var m in medications)
                {
                    Console.WriteLine($"ID: {m.Id}");
                    Console.WriteLine($"Name: {m.Name}");
                    Console.WriteLine($"Dosage: {m.Dosage}");
                    Console.WriteLine($"Frequency: {m.Frequency}");
                    Console.WriteLine($"Patient: {m.Patient.FirstName} {m.Patient.LastName}");
                    if (m.Disease != null)
                        Console.WriteLine($"For Disease: {m.Disease.Name}");
                    if (!string.IsNullOrEmpty(m.Notes))
                        Console.WriteLine($"Notes: {m.Notes}");
                    Console.WriteLine("───────────────────────────────────────");
                }
                Console.WriteLine($"\nTotal: {medications.Count} medications");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdateMedication(MedicationService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         UPDATE MEDICATION");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Medication ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var medication = await service.GetByIdAsync(id);
            if (medication == null)
            {
                Console.WriteLine("❌ Medication not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent data: {medication.Name}");
            Console.WriteLine("(Press Enter to keep current value)\n");

            Console.Write($"Dosage [{medication.Dosage}]: ");
            var dosage = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dosage))
                medication.Dosage = dosage;

            Console.Write($"Frequency [{medication.Frequency}]: ");
            var frequency = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(frequency))
                medication.Frequency = frequency;

            Console.Write($"Notes [{medication.Notes}]: ");
            var notes = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(notes))
                medication.Notes = notes;

            await service.UpdateAsync(medication);
            Console.WriteLine("\n✅ Medication updated successfully!");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task DeleteMedication(MedicationService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         DELETE MEDICATION");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Medication ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var medication = await service.GetByIdAsync(id);
            if (medication == null)
            {
                Console.WriteLine("❌ Medication not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nAre you sure you want to delete:");
            Console.WriteLine($"{medication.Name} - {medication.Dosage}?");
            Console.Write("\nType 'YES' to confirm: ");

            if (Console.ReadLine()?.ToUpper() == "YES")
            {
                await service.DeleteAsync(medication);
                Console.WriteLine("\n✅ Medication deleted successfully!");
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