using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.App.Repositories
{
    public class DiseaseRepository
    {
        private readonly MedicalDbContext _context;

        public DiseaseRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public void Add(Disease disease)
        {
            _context.Diseases.Add(disease);
            _context.SaveChanges();
            Console.WriteLine($"✅ Disease '{disease.Name}' added successfully!");
        }

        public List<Disease> GetAll()
        {
            return _context.Diseases
                .Include(d => d.Patient)
                .ToList();
        }

        public List<Disease> GetByPatientId(int patientId)
        {
            return _context.Diseases
                .Where(d => d.PatientId == patientId)
                .ToList();
        }

        public Disease? GetById(int id)
        {
            return _context.Diseases
                .Include(d => d.Patient)
                .FirstOrDefault(d => d.Id == id);
        }

        public void Update(Disease disease)
        {
            _context.Diseases.Update(disease);
            _context.SaveChanges();
            Console.WriteLine($"✅ Disease '{disease.Name}' updated successfully!");
        }

        public void Delete(int id)
        {
            var disease = _context.Diseases.Find(id);
            if (disease != null)
            {
                _context.Diseases.Remove(disease);
                _context.SaveChanges();
                Console.WriteLine($"✅ Disease '{disease.Name}' deleted successfully!");
            }
            else
            {
                Console.WriteLine("❌ Disease not found!");
            }
        }

        public List<Disease> GetActiveByPatientId(int patientId)
        {
            return _context.Diseases
                .Where(d => d.PatientId == patientId && d.EndDate == null)
                .ToList();
        }
    }
}