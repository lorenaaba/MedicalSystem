using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using MedicalSystem.App.Services;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.App.UI.Menus
{
    public class SpecialistExamMenu
    {
        private readonly string _connectionString;

        public SpecialistExamMenu(string connectionString)
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
                Console.WriteLine("║      SPECIALIST EXAM MANAGEMENT                ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Schedule New Exam                          ║");
                Console.WriteLine("║ 2. List All Exams                             ║");
                Console.WriteLine("║ 3. Update Exam                                ║");
                Console.WriteLine("║ 4. Delete Exam                                ║");
                Console.WriteLine("╠════════════════════════════════════════════════╣");
                Console.WriteLine("║ 0. Back                                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.Write("\nSelect option: ");

                var choice = Console.ReadLine();

                var options = new DbContextOptionsBuilder<MedicalDbContext>()
                    .UseNpgsql(_connectionString)
                    .Options;

                using var context = new MedicalDbContext(options);
                var service = new SpecialistExamService(context);
                var patientService = new PatientService(context);
                var doctorService = new DoctorService(context);

                switch (choice)
                {
                    case "1":
                        await CreateSpecialistExam(service, patientService, doctorService);
                        break;
                    case "2":
                        await ListSpecialistExams(service);
                        break;
                    case "3":
                        await UpdateSpecialistExam(service);
                        break;
                    case "4":
                        await DeleteSpecialistExam(service);
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

        private async Task CreateSpecialistExam(SpecialistExamService service, PatientService patientService, DoctorService doctorService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       SCHEDULE SPECIALIST EXAM");
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

                Console.Write("Doctor ID: ");
                if (!int.TryParse(Console.ReadLine(), out int doctorId))
                {
                    Console.WriteLine("❌ Invalid Doctor ID!");
                    Console.ReadKey();
                    return;
                }

                var doctor = await doctorService.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    Console.WriteLine("❌ Doctor not found!");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Doctor: Dr. {doctor.FirstName} {doctor.LastName} ({doctor.Specialization})\n");

                Console.WriteLine("Exam Types:");
                Console.WriteLine("1. CT  2. MR  3. ULTRA  4. EKG  5. ECHO");
                Console.WriteLine("6. EYE (OKO)  7. DERM  8. DENTAL  9. MAMMO  10. EEG");
                Console.Write("\nSelect exam type (1-10): ");
                if (!int.TryParse(Console.ReadLine(), out int examTypeChoice) || examTypeChoice < 1 || examTypeChoice > 10)
                {
                    Console.WriteLine("❌ Invalid choice!");
                    Console.ReadKey();
                    return;
                }

                ExamType examType = (ExamType)(examTypeChoice - 1);

                Console.Write("Scheduled Date/Time (yyyy-MM-dd HH:mm): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime scheduledAt))
                {
                    Console.WriteLine("❌ Invalid date/time format!");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Notes [Optional]: ");
                var notes = Console.ReadLine();

                var exam = new SpecialistExam
                {
                    ExamType = examType,
                    ScheduledAt = scheduledAt,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    Notes = notes
                };

                await service.CreateAsync(exam);
                Console.WriteLine($"\n✅ Exam scheduled successfully! ID: {exam.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task ListSpecialistExams(SpecialistExamService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("       ALL SPECIALIST EXAMS");
            Console.WriteLine("═══════════════════════════════════════\n");

            var exams = await service.GetAllAsync();

            if (exams.Count == 0)
            {
                Console.WriteLine("No exams found.");
            }
            else
            {
                foreach (var e in exams)
                {
                    Console.WriteLine($"ID: {e.Id}");
                    Console.WriteLine($"Type: {e.ExamType}");
                    Console.WriteLine($"Patient: {e.Patient.FirstName} {e.Patient.LastName}");
                    Console.WriteLine($"Doctor: Dr. {e.Doctor.FirstName} {e.Doctor.LastName} ({e.Doctor.Specialization})");
                    Console.WriteLine($"Scheduled: {e.ScheduledAt:yyyy-MM-dd HH:mm}");
                    if (!string.IsNullOrEmpty(e.Notes))
                        Console.WriteLine($"Notes: {e.Notes}");
                    Console.WriteLine("───────────────────────────────────────");
                }
                Console.WriteLine($"\nTotal: {exams.Count} exams");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task UpdateSpecialistExam(SpecialistExamService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         UPDATE SPECIALIST EXAM");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Exam ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var exam = await service.GetByIdAsync(id);
            if (exam == null)
            {
                Console.WriteLine("❌ Exam not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent: {exam.ExamType} at {exam.ScheduledAt:yyyy-MM-dd HH:mm}");
            Console.WriteLine("(Press Enter to keep current value)\n");

            Console.Write($"New Date/Time [{exam.ScheduledAt:yyyy-MM-dd HH:mm}]: ");
            var dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out DateTime newDate))
            {
                exam.ScheduledAt = newDate;
            }

            Console.Write($"Notes [{exam.Notes}]: ");
            var notes = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(notes))
                exam.Notes = notes;

            await service.UpdateAsync(exam);
            Console.WriteLine("\n✅ Exam updated successfully!");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private async Task DeleteSpecialistExam(SpecialistExamService service)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         DELETE SPECIALIST EXAM");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Enter Exam ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                Console.ReadKey();
                return;
            }

            var exam = await service.GetByIdAsync(id);
            if (exam == null)
            {
                Console.WriteLine("❌ Exam not found!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nAre you sure you want to delete:");
            Console.WriteLine($"{exam.ExamType} for {exam.Patient.FirstName} {exam.Patient.LastName}");
            Console.WriteLine($"Scheduled: {exam.ScheduledAt:yyyy-MM-dd HH:mm}?");
            Console.Write("\nType 'YES' to confirm: ");

            if (Console.ReadLine()?.ToUpper() == "YES")
            {
                await service.DeleteAsync(exam);
                Console.WriteLine("\n✅ Exam deleted successfully!");
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