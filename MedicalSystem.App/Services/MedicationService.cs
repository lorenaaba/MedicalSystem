using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSystem.App.Services
{
    public class MedicationService
    {
        private readonly MedicalDbContext _context;

        public MedicationService(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<Medication> CreateAsync(Medication medication)
        {
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();
            return medication;
        }

        public async Task<List<Medication>> GetAllAsync()
        {
            return await _context.Medications
                .Include(m => m.Patient)
                .Include(m => m.Disease)
                .OrderBy(m => m.Patient.LastName)
                .ToListAsync();
        }

        public async Task<Medication?> GetByIdAsync(int id)
        {
            return await _context.Medications
                .Include(m => m.Patient)
                .Include(m => m.Disease)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateAsync(Medication medication)
        {
            _context.Medications.Update(medication);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Medication medication)
        {
            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
        }
    }
}