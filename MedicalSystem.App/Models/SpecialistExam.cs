using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystem.App.Models
{
    [Table("specialist_exams")]
    public class SpecialistExam
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("exam_type")]
        public ExamType ExamType { get; set; }

        [Required]
        [Column("scheduled_date")] 
        public DateTime ScheduledAt { get; set; }

        [Column("notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column("completed")]
        public bool Completed { get; set; } = false;

        [Required]
        [Column("patient_id")]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        [Required]
        [Column("doctor_id")]
        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;
    }
}