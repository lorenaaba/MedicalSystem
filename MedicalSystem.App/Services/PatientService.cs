using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSystem.App.Services
{
    public class PatientService
    {
        private readonly MedicalDbContext _context;

        public PatientService(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<List<Patient>> GetAllAsync()
        {
            return await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
        }

        public async Task<Patient?> GetByOibAsync(string oib)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.Oib == oib);
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<bool> ExistsWithOibAsync(string oib)
        {
            return await _context.Patients.AnyAsync(p => p.Oib == oib);
        }

        public async Task UpdateAsync(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Patient patient)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }
    }
}