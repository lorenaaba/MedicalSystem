using SimpleMedicalORM.Core;
using MedicalSystem.App.CustomOrm.Context;
using MedicalSystem.App.CustomOrm.Entities;

namespace MedicalSystem.App.Demos
{
    public class CustomOrmExpressionDemo
    {
        private readonly string _connectionString;

        public CustomOrmExpressionDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║  CUSTOM ORM - EXPRESSION & FILTERING DEMO      ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var context = new MedicalOrmContext(_connectionString);

            try
            {
                await context.EnsureCreatedAsync();

                Console.WriteLine(" Preparing test data...\n");

                var patients = new List<PatientEntity>
                {
                    new PatientEntity
                    {
                        FirstName = "Ana",
                        LastName = "Horvat",
                        OIB = GenerateRandomOIB(),
                        DateOfBirth = new DateTime(1990, 3, 15),
                        Gender = "F",
                        Phone = "091-111-2222",
                        Email = "ana@example.com",
                        Address = "Ilica 1"
                    },
                    new PatientEntity
                    {
                        FirstName = "Marko",
                        LastName = "Kovač",
                        OIB = GenerateRandomOIB(),
                        DateOfBirth = new DateTime(1985, 7, 20),
                        Gender = "M",
                        Phone = "091-333-4444",
                        Email = "marko@example.com",
                        Address = "Savska 2"
                    },
                    new PatientEntity
                    {
                        FirstName = "Petra",
                        LastName = "Marić",
                        OIB = GenerateRandomOIB(),
                        DateOfBirth = new DateTime(1995, 11, 5),
                        Gender = "F",
                        Phone = "091-555-6666",
                        Email = "petra@example.com",
                        Address = "Vlaška 3"
                    },
                    new PatientEntity
                    {
                        FirstName = "Ivan",
                        LastName = "Novak",
                        OIB = GenerateRandomOIB(),
                        DateOfBirth = new DateTime(1988, 2, 28),
                        Gender = "M",
                        Phone = "091-777-8888",
                        Email = "ivan@example.com",
                        Address = "Maksimirska 4"
                    }
                };

                foreach (var p in patients)
                {
                    context.Patients.Add(p);
                }
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Created {patients.Count} test patients\n");

                Console.WriteLine(" Expression Filtering Demo:\n");

                Console.WriteLine("1️⃣  Filter by Gender (Female):");
                var femalePatients = await context.Patients
                    .Where(p => p.Gender == "F")
                    .ToListAsync();

                foreach (var p in femalePatients)
                {
                    Console.WriteLine($"   • {p.FirstName} {p.LastName} - {p.Email}");
                }
                Console.WriteLine($"   Found: {femalePatients.Count} female patients\n");

                Console.WriteLine("2️⃣  Filter by Gender (Male):");
                var malePatients = await context.Patients
                    .Where(p => p.Gender == "M")
                    .ToListAsync();

                foreach (var p in malePatients)
                {
                    Console.WriteLine($"   • {p.FirstName} {p.LastName} - {p.Email}");
                }
                Console.WriteLine($"   Found: {malePatients.Count} male patients\n");

                Console.WriteLine("3️⃣  List all patients (no filter):");
                var allPatients = await context.Patients.ToListAsync();
                foreach (var p in allPatients)
                {
                    Console.WriteLine($"   • {p.FirstName} {p.LastName} - {p.Gender} - {p.Phone}");
                }
                Console.WriteLine($"   Total: {allPatients.Count} patients\n");

                Console.WriteLine("🧹 Cleaning up test data...");
                foreach (var p in patients)
                {
                    context.Patients.Remove(p);
                }
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Test data cleaned up");

                Console.WriteLine("\n✅ Expression & Filtering demo completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            }
            finally
            {
                context.Dispose();
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private string GenerateRandomOIB()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 11);
        }
    }
}