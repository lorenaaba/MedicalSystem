using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using MedicalSystem.App.Services;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.App.UI.Menus
{
    public class PatientMenu
    {
        private readonly string _connectionString;

        public PatientMenu(string connectionString)
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
                Console.WriteLine("║         PATIENT MANAGEMENT                     ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Create New Patient                         ║");
                Console.WriteLine("║ 2. List All Patients                          ║");
                Console.WriteLine("║ 3. Find Patient by OIB                        ║");
                Console.WriteLine("║ 4. Update Patient                             ║");
                Console.WriteLine("║ 5. Delete Patient                             ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 0. Back                                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.Write("\nSelect option: ");

                var choice = Console.ReadLine();

                var options = new DbContextOptionsBuilder<MedicalDbContext>()
                    .UseNpgsql(_connectionString)
                    .Options;

                using var context = new MedicalDbContext(options);
                var service = new PatientService(context);

                switch (choice)
                {
                    case "1":
                        await CreatePatient(service);
                        break;
                    case "2":
                        await ListPatients(service);
                        break;
                    case "3":
                        await FindPatientByOIB(service);
                        break;
                    case "4":
                        await UpdatePatient(service);
                        break;
                    case "5":
                        await DeletePatient(service);
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

        private async Task CreatePatient(PatientService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       CREATE NEW PATIENT");
            Console.WriteLine("═══════════════════════════════════════\n");

            try
            {
                Console.Write("First Name: ");
                var firstName = Console.ReadLine();

                Console.Write("Last Name: ");
                var lastName = Console.ReadLine();

                Console.Write("OIB (11 digits): ");
                var oib = Console.ReadLine();

                if (await service.ExistsWithOibAsync(oib!))
                {
                    Console.WriteLine("\n❌ Patient with this OIB already exists!");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Date of Birth (yyyy-MM-dd): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime dob))
                {
                    Console.WriteLine("❌ Invalid date format!");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Gender (M/F): ");
                var gender = Console.ReadLine()?.ToUpper();
                if (gender != "M" && gender != "F")
                {
                    Console.WriteLine("❌ Gender must be M or F!");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Residence Address: ");
                var residenceAddress = Console.ReadLine();

                Console.Write("Permanent Address: ");
                var permanentAddress = Console.ReadLine();

                var patient = new Patient
                {
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    Oib = oib ?? "",
                    DateOfBirth = dob,
                    Gender = gender,
                    ResidenceAddress = residenceAddress ?? "",
                    PermanentAddress = permanentAddress ?? ""
                };

                await service.CreateAsync(patient);
                Console.WriteLine($"\n✅ Patient created successfully! ID: {patient.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task ListPatients(PatientService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ALL PATIENTS");
            Console.WriteLine("═══════════════════════════════════════\n");

            var patients = await service.GetAllAsync();

            if (patients.Count == 0)
            {
                Console.WriteLine("No patients found.");
            }
            else
            {
                foreach (var p in patients)
                {
                    Console.WriteLine($"ID: {p.Id}");
                    Console.WriteLine($"Name: {p.FirstName} {p.LastName}");
                    Console.WriteLine($"OIB: {p.Oib}");
                    Console.WriteLine($"Born: {p.DateOfBirth:yyyy-MM-dd}");
                    Console.WriteLine($"Gender: {p.Gender}");
                    Console.WriteLine($"Address: {p.ResidenceAddress}");
                    Console.WriteLine("───────────────────────────────────────");
                }
                Console.WriteLine($"\nTotal: {patients.Count} patients");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task FindPatientByOIB(PatientService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       FIND PATIENT BY OIB");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter OIB: ");
            var oib = Console.ReadLine();

            var patient = await service.GetByOibAsync(oib!);

            if (patient == null)
            {
                Console.WriteLine("\n❌ Patient not found!");
            }
            else
            {
                Console.WriteLine("\n✅ Patient found:\n");
                Console.WriteLine($"ID: {patient.Id}");
                Console.WriteLine($"Name: {patient.FirstName} {patient.LastName}");
                Console.WriteLine($"OIB: {patient.Oib}");
                Console.WriteLine($"Born: {patient.DateOfBirth:yyyy-MM-dd}");
                Console.WriteLine($"Gender: {patient.Gender}");
                Console.WriteLine($"Residence: {patient.ResidenceAddress}");
                Console.WriteLine($"Permanent: {patient.PermanentAddress}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdatePatient(PatientService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         UPDATE PATIENT");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Patient ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var patient = await service.GetByIdAsync(id);
            if (patient == null)
            {
                Console.WriteLine("❌ Patient not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent data for: {patient.FirstName} {patient.LastName}");
            Console.WriteLine("(Press Enter to keep current value)\n");

            Console.Write($"First Name [{patient.FirstName}]: ");
            var firstName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(firstName))
                patient.FirstName = firstName;

            Console.Write($"Last Name [{patient.LastName}]: ");
            var lastName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(lastName))
                patient.LastName = lastName;

            Console.Write($"Residence Address [{patient.ResidenceAddress}]: ");
            var residence = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(residence))
                patient.ResidenceAddress = residence;

            Console.Write($"Permanent Address [{patient.PermanentAddress}]: ");
            var permanent = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(permanent))
                patient.PermanentAddress = permanent;

            await service.UpdateAsync(patient);
            Console.WriteLine("\n✅ Patient updated successfully!");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task DeletePatient(PatientService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         DELETE PATIENT");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Patient ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var patient = await service.GetByIdAsync(id);
            if (patient == null)
            {
                Console.WriteLine("❌ Patient not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nAre you sure you want to delete:");
            Console.WriteLine($"{patient.FirstName} {patient.LastName} (OIB: {patient.Oib})?");
            Console.Write("\nType 'YES' to confirm: ");

            if (Console.ReadLine()?.ToUpper() == "YES")
            {
                await service.DeleteAsync(patient);
                Console.WriteLine("\n✅ Patient deleted successfully!");
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