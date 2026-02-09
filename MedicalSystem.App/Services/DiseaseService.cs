using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSystem.App.Services
{
    public class DiseaseService
    {
        private readonly MedicalDbContext _context;

        public DiseaseService(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<Disease> CreateAsync(Disease disease)
        {
            _context.Diseases.Add(disease);
            await _context.SaveChangesAsync();
            return disease;
        }

        public async Task<List<Disease>> GetAllAsync()
        {
            return await _context.Diseases
                .Include(d => d.Patient)
                .OrderBy(d => d.Patient.LastName)
                .ToListAsync();
        }

        public async Task<Disease?> GetByIdAsync(int id)
        {
            return await _context.Diseases
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateAsync(Disease disease)
        {
            _context.Diseases.Update(disease);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Disease disease)
        {
            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();
        }
    }
}