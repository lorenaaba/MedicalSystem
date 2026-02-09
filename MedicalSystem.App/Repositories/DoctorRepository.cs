using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.App.Repositories
{
    public class DoctorRepository
    {
        private readonly MedicalDbContext _context;

        public DoctorRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public List<Doctor> GetAll()
        {
            return _context.Doctors
                .Include(d => d.SpecialistExams)
                .ToList();
        }

        public Doctor? GetById(int id)
        {
            return _context.Doctors
                .Include(d => d.SpecialistExams)
                .FirstOrDefault(d => d.Id == id);
        }

        public List<Doctor> GetBySpecialization(string specialization)
        {
            return _context.Doctors
                .Where(d => d.Specialization.Contains(specialization))
                .ToList();
        }
    }
}