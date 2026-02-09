using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSystem.App.Services
{
    public class SpecialistExamService
    {
        private readonly MedicalDbContext _context;

        public SpecialistExamService(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<SpecialistExam> CreateAsync(SpecialistExam exam)
        {
            _context.SpecialistExams.Add(exam);
            await _context.SaveChangesAsync();
            return exam;
        }

        public async Task<List<SpecialistExam>> GetAllAsync()
        {
            return await _context.SpecialistExams
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .OrderBy(e => e.ScheduledAt)
                .ToListAsync();
        }

        public async Task<SpecialistExam?> GetByIdAsync(int id)
        {
            return await _context.SpecialistExams
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(SpecialistExam exam)
        {
            _context.SpecialistExams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SpecialistExam exam)
        {
            _context.SpecialistExams.Remove(exam);
            await _context.SaveChangesAsync();
        }
    }
}