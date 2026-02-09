using SimpleMedicalORM.Connection;
using SimpleMedicalORM.Core;
using MedicalSystem.App.CustomOrm.Context;
using MedicalSystem.App.CustomOrm.Entities;

namespace MedicalSystem.App.Demos
{
    public class CustomOrmBasicDemo
    {
        private readonly string _connectionString;

        public CustomOrmBasicDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║    CUSTOM ORM - BASIC CRUD DEMO                ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var connection = new PostgresConnection(_connectionString);
            var context = new MedicalOrmContext(_connectionString);

            try
            {
                try
                {
                    await connection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS custom_medications CASCADE");
                    await connection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS custom_diseases CASCADE");
                    await connection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS custom_patients CASCADE");
                    await connection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS custom_doctors CASCADE");
                    Console.WriteLine("🧹 Cleared existing custom tables\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Cleanup warning: {ex.Message}\n");
                }

                await context.EnsureCreatedAsync();

                Console.WriteLine(" CRUD Operations Demo\n");

                Console.WriteLine("1️⃣  CREATE - Adding new patient...");
                var patient = new PatientEntity
                {
                    FirstName = "Test",
                    LastName = "Patient",
                    OIB = GenerateRandomOIB(),
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "M",
                    Phone = "091-123-4567",
                    Email = "test@example.com",
                    Address = "Test Address 123"
                };

                context.Patients.Add(patient);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Patient created with ID: {patient.PatientId}\n");

                Console.WriteLine("2️⃣  READ - Finding patient...");
                var foundPatient = await context.Patients.FindAsync(patient.PatientId);
                if (foundPatient != null)
                {
                    Console.WriteLine($"✅ Found: {foundPatient.FirstName} {foundPatient.LastName}");
                    Console.WriteLine($"   OIB: {foundPatient.OIB}");
                    Console.WriteLine($"   Phone: {foundPatient.Phone}");
                    Console.WriteLine($"   Email: {foundPatient.Email}\n");
                }

                Console.WriteLine("3️⃣  UPDATE - Modifying patient...");
                foundPatient!.FirstName = "Updated";
                foundPatient.Phone = "091-999-8888";
                
                context.ChangeTracker.SetState(foundPatient, EntityState.Modified);
                
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Patient updated!\n");

                Console.WriteLine("4️⃣  VERIFY - Reading updated patient...");
                var updatedPatient = await context.Patients.FindAsync(patient.PatientId);
                Console.WriteLine($"   Name: {updatedPatient?.FirstName}");
                Console.WriteLine($"   Phone: {updatedPatient?.Phone}\n");

                Console.WriteLine("5️⃣  DELETE - Removing patient...");
                context.Patients.Remove(updatedPatient!);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Patient deleted!\n");

                Console.WriteLine("6️⃣  VERIFY - Checking deletion...");
                var deletedPatient = await context.Patients.FindAsync(patient.PatientId);
                Console.WriteLine($"   {(deletedPatient == null ? "✅ Successfully deleted" : "❌ Still exists")}\n");

                Console.WriteLine("✅ Custom ORM CRUD demo completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            }
            finally
            {
                connection.Dispose();
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