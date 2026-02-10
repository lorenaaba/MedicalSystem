using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.App.Repositories
{
    public class PatientRepository
    {
        private readonly MedicalDbContext _context;

        public PatientRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public void Add(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();
            Console.WriteLine($"✅ Patient {patient.FirstName} {patient.LastName} added successfully!");
        }

        public List<Patient> GetAll()
        {
            return _context.Patients
                .Include(p => p.Diseases)
                .Include(p => p.Medications)
                .Include(p => p.SpecialistExams)
                .ToList();
        }

        public Patient? GetById(int id)
        {
            return _context.Patients
                .Include(p => p.Diseases)
                .Include(p => p.Medications)
                .Include(p => p.SpecialistExams)
                .FirstOrDefault(p => p.Id == id);
        }

        public Patient? GetByOib(string oib)
        {
            return _context.Patients
                .FirstOrDefault(p => p.Oib == oib);
        }

        public void Update(Patient patient)
        {
            _context.Patients.Update(patient);
            _context.SaveChanges();
            Console.WriteLine($"✅ Patient {patient.FirstName} {patient.LastName} updated successfully!");
        }

        public void Delete(int id)
        {
            var patient = _context.Patients.Find(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                _context.SaveChanges();
                Console.WriteLine($"✅ Patient {patient.FirstName} {patient.LastName} deleted successfully!");
            }
            else
            {
                Console.WriteLine("❌ Patient not found!");
            }
        }

        public List<Patient> Search(string searchTerm)
        {
            return _context.Patients
                .Where(p => p.FirstName.Contains(searchTerm)
                         || p.LastName.Contains(searchTerm)
                         || p.Oib.Contains(searchTerm))
                .ToList();
        }
    }
}