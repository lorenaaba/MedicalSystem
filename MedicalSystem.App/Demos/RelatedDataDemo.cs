using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.App.Demos
{
    public class RelatedDataDemo
    {
        private readonly string _connectionString;

        public RelatedDataDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║      EF CORE - RELATED DATA DEMO               ║");
            Console.WriteLine("║   (Eager Loading & Navigation Properties)     ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var options = new DbContextOptionsBuilder<MedicalDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new MedicalDbContext(options);

            try
            {
                Console.WriteLine(" Creating patient with related data...\n");

                var patient = new Patient
                {
                    FirstName = "Ana",
                    LastName = "Anić",
                    Oib = Guid.NewGuid().ToString("N").Substring(0, 11),
                    DateOfBirth = new DateTime(1985, 3, 20),
                    Gender = "F",
                    ResidenceAddress = "Savska 45, Zagreb",
                    PermanentAddress = "Savska 45, Zagreb"
                };

                context.Patients.Add(patient);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Patient created: {patient.FirstName} {patient.LastName} (ID: {patient.Id})");

                var disease1 = new Disease
                {
                    Name = "Hipertenzija",
                    StartDate = DateTime.Now.AddMonths(-6),
                    PatientId = patient.Id
                };

                var disease2 = new Disease
                {
                    Name = "Dijabetes tip 2",
                    StartDate = DateTime.Now.AddYears(-2),
                    PatientId = patient.Id
                };

                context.Diseases.AddRange(disease1, disease2);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Added 2 diseases");

                var medication1 = new Medication
                {
                    Name = "Lisinopril",
                    Dosage = "10mg",
                    Frequency = "1x dnevno ujutro",
                    PatientId = patient.Id,
                    DiseaseId = disease1.Id
                };

                var medication2 = new Medication
                {
                    Name = "Metformin",
                    Dosage = "500mg",
                    Frequency = "2x dnevno s jelom",
                    PatientId = patient.Id,
                    DiseaseId = disease2.Id
                };

                context.Medications.AddRange(medication1, medication2);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Added 2 medications");

                var doctor = await context.Doctors.FirstAsync();

                var exam = new SpecialistExam
                {
                    ExamType = ExamType.EKG,
                    ScheduledAt = DateTime.Now.AddDays(7),
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    Notes = "Kontrolni pregled srca"
                };

                context.SpecialistExams.Add(exam);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Scheduled specialist exam\n");

                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("📖 EAGER LOADING - Loading patient with all related data...\n");

                var patientWithData = await context.Patients
                    .Include(p => p.Diseases)
                    .Include(p => p.Medications)
                        .ThenInclude(m => m.Disease)
                    .Include(p => p.SpecialistExams)
                        .ThenInclude(e => e.Doctor)
                    .FirstOrDefaultAsync(p => p.Id == patient.Id);

                if (patientWithData != null)
                {
                    Console.WriteLine($" Patient: {patientWithData.FirstName} {patientWithData.LastName}");
                    Console.WriteLine($"   OIB: {patientWithData.Oib}");
                    Console.WriteLine($"   Born: {patientWithData.DateOfBirth:yyyy-MM-dd}");
                    Console.WriteLine($"   Address: {patientWithData.ResidenceAddress}\n");

                    Console.WriteLine($" Diseases ({patientWithData.Diseases.Count}):");
                    foreach (var d in patientWithData.Diseases)
                    {
                        var duration = d.EndDate.HasValue
                            ? $"{d.StartDate:yyyy-MM-dd} to {d.EndDate:yyyy-MM-dd}"
                            : $"Since {d.StartDate:yyyy-MM-dd} (ongoing)";
                        Console.WriteLine($"   • {d.Name} - {duration}");
                    }

                    Console.WriteLine($"\n Medications ({patientWithData.Medications.Count}):");
                    foreach (var med in patientWithData.Medications)
                    {
                        var diseaseInfo = med.Disease != null ? $" (for {med.Disease.Name})" : "";
                        Console.WriteLine($"   • {med.Name} - {med.Dosage}, {med.Frequency}{diseaseInfo}");
                    }

                    Console.WriteLine($"\n Specialist Exams ({patientWithData.SpecialistExams.Count}):");
                    foreach (var e in patientWithData.SpecialistExams)
                    {
                        Console.WriteLine($"   • {e.ExamType} - {e.ScheduledAt:yyyy-MM-dd HH:mm}");
                        Console.WriteLine($"     Doctor: Dr. {e.Doctor.FirstName} {e.Doctor.LastName} ({e.Doctor.Specialization})");
                        if (!string.IsNullOrEmpty(e.Notes))
                            Console.WriteLine($"     Notes: {e.Notes}");
                    }
                }

                Console.WriteLine("\n✅ Related data demonstration completed!");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"\n❌ Database Error: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}