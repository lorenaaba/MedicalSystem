using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSystem.App.Services
{
    public class DoctorService
    {
        private readonly MedicalDbContext _context;

        public DoctorService(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task SeedInitialDoctorsAsync()
        {
            try
            {
                if (!await _context.Doctors.AnyAsync())
                {
                    Console.WriteLine(" Seeding initial doctors...");

                    var doctors = new List<Doctor>
                    {
                        new Doctor { FirstName = "Ivan", LastName = "Horvat", Specialization = "Cardiology" },
                        new Doctor { FirstName = "Marija", LastName = "Kovač", Specialization = "Radiology" },
                        new Doctor { FirstName = "Petar", LastName = "Novak", Specialization = "Neurology" },
                        new Doctor { FirstName = "Ana", LastName = "Marić", Specialization = "Dermatology" },
                        new Doctor { FirstName = "Luka", LastName = "Babić", Specialization = "Ophthalmology" }
                    };

                    _context.Doctors.AddRange(doctors);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ {doctors.Count} doctors seeded\n");
                }
                else
                {
                    Console.WriteLine($"  Doctors already exist ({await _context.Doctors.CountAsync()} in database)\n");
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"❌ Error seeding doctors: {ex.Message}");
            }
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _context.Doctors.ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _context.Doctors.FindAsync(id);
        }
    }
}