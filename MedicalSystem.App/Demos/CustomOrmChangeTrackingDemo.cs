using SimpleMedicalORM.Core;
using MedicalSystem.App.CustomOrm.Context;
using MedicalSystem.App.CustomOrm.Entities;

namespace MedicalSystem.App.Demos
{
    public class CustomOrmChangeTrackingDemo
    {
        private readonly string _connectionString;

        public CustomOrmChangeTrackingDemo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║   CUSTOM ORM - CHANGE TRACKING DEMO            ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            var context = new MedicalOrmContext(_connectionString);

            try
            {
                await context.EnsureCreatedAsync();

                Console.WriteLine(" Demonstrating Change Tracking...\n");

                Console.WriteLine("1️⃣  Creating new patient...");
                var patient = new PatientEntity
                {
                    FirstName = "Change",
                    LastName = "Tracking",
                    OIB = GenerateRandomOIB(),
                    DateOfBirth = new DateTime(1995, 5, 15),
                    Gender = "F",
                    Phone = "091-555-6666",
                    Email = "change@tracking.com",
                    Address = "Change Street 1"
                };

                context.Patients.Add(patient);
                Console.WriteLine($"   Initial State: {context.ChangeTracker.GetState(patient)}");

                await context.SaveChangesAsync();
                Console.WriteLine($"   After Save State: {context.ChangeTracker.GetState(patient)}");
                Console.WriteLine($"✅ Patient created with ID: {patient.PatientId}\n");

                Console.WriteLine("2️⃣  Modifying patient (without explicit tracking)...");
                Console.WriteLine($"   Current State: {context.ChangeTracker.GetState(patient)}");

                patient.FirstName = "Modified";
                patient.LastName = "Name";
                patient.Phone = "091-777-8888";

                Console.WriteLine($"   State after changes: {context.ChangeTracker.GetState(patient)}");
                Console.WriteLine("   Calling DetectChanges()...");
                context.ChangeTracker.DetectChanges();
                Console.WriteLine($"   State after DetectChanges: {context.ChangeTracker.GetState(patient)}");

                await context.SaveChangesAsync();
                Console.WriteLine($"   State after SaveChanges: {context.ChangeTracker.GetState(patient)}");
                Console.WriteLine("✅ Changes saved!\n");

                var verifiedPatient = await context.Patients.FindAsync(patient.PatientId);
                Console.WriteLine($"   Verified: {verifiedPatient?.FirstName} {verifiedPatient?.LastName}");
                Console.WriteLine($"   Phone: {verifiedPatient?.Phone}\n");

                Console.WriteLine("3️⃣  Marking for deletion...");
                context.ChangeTracker.SetState(patient, EntityState.Deleted);
                Console.WriteLine($"   State: {context.ChangeTracker.GetState(patient)}");

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Patient deleted!\n");

                Console.WriteLine(" Change Tracking Summary:");
                Console.WriteLine("   States demonstrated:");
                Console.WriteLine("   • Added → Unchanged (after create)");
                Console.WriteLine("   • Unchanged → Modified (after detect changes)");
                Console.WriteLine("   • Modified → Unchanged (after save)");
                Console.WriteLine("   • Deleted → executed (after save)");

                Console.WriteLine("\n✅ Change Tracking demo completed!");
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