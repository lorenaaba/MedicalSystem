using MedicalSystem.App.Data;
using MedicalSystem.App.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.App.Repositories
{
    public class SpecialistExamRepository
    {
        private readonly MedicalDbContext _context;

        public SpecialistExamRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public void Add(SpecialistExam exam)
        {
            _context.SpecialistExams.Add(exam);
            _context.SaveChanges();
            Console.WriteLine($"✅ Specialist exam scheduled successfully!");
        }

        public List<SpecialistExam> GetAll()
        {
            return _context.SpecialistExams
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .ToList();
        }

        public List<SpecialistExam> GetByPatientId(int patientId)
        {
            return _context.SpecialistExams
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == patientId)
                .ToList();
        }

        public List<SpecialistExam> GetByDoctorId(int doctorId)
        {
            return _context.SpecialistExams
                .Include(e => e.Patient)
                .Where(e => e.DoctorId == doctorId)
                .ToList();
        }

        public SpecialistExam? GetById(int id)
        {
            return _context.SpecialistExams
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .FirstOrDefault(e => e.Id == id);
        }

        public void Update(SpecialistExam exam)
        {
            _context.SpecialistExams.Update(exam);
            _context.SaveChanges();
            Console.WriteLine($"✅ Specialist exam updated successfully!");
        }

        public void Delete(int id)
        {
            var exam = _context.SpecialistExams.Find(id);
            if (exam != null)
            {
                _context.SpecialistExams.Remove(exam);
                _context.SaveChanges();
                Console.WriteLine($"✅ Specialist exam deleted successfully!");
            }
            else
            {
                Console.WriteLine("❌ Exam not found!");
            }
        }

        public List<SpecialistExam> GetUpcomingByPatientId(int patientId)
        {
            return _context.SpecialistExams
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == patientId
                         && e.ScheduledAt > DateTime.Now)  
                .OrderBy(e => e.ScheduledAt) 
                .ToList();
        }
    }
}