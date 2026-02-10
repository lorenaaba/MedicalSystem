using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.App.Repositories
{
    public class MedicationRepository
    {
        private readonly MedicalDbContext _context;

        public MedicationRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public void Add(Medication medication)
        {
            _context.Medications.Add(medication);
            _context.SaveChanges();
            Console.WriteLine($"✅ Medication '{medication.Name}' added successfully!");
        }

        public List<Medication> GetAll()
        {
            return _context.Medications
                .Include(m => m.Patient)
                .Include(m => m.Disease)
                .ToList();
        }

        public List<Medication> GetByPatientId(int patientId)
        {
            return _context.Medications
                .Include(m => m.Disease)
                .Where(m => m.PatientId == patientId)
                .ToList();
        }

        public Medication? GetById(int id)
        {
            return _context.Medications
                .Include(m => m.Patient)
                .Include(m => m.Disease)
                .FirstOrDefault(m => m.Id == id);
        }

        public void Update(Medication medication)
        {
            _context.Medications.Update(medication);
            _context.SaveChanges();
            Console.WriteLine($"✅ Medication '{medication.Name}' updated successfully!");
        }

        public void Delete(int id)
        {
            var medication = _context.Medications.Find(id);
            if (medication != null)
            {
                _context.Medications.Remove(medication);
                _context.SaveChanges();
                Console.WriteLine($"✅ Medication '{medication.Name}' deleted successfully!");
            }
            else
            {
                Console.WriteLine("❌ Medication not found!");
            }
        }
    }
}