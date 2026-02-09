using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystem.App.Models
{
    [Table("medications")]
    public class Medication
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("dosage")]
        public string Dosage { get; set; } = string.Empty; 

        [Required]
        [MaxLength(100)]
        [Column("frequency")]
        public string Frequency { get; set; } = string.Empty; 

        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; } 

        [Required]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("disease_id")]
        public int? DiseaseId { get; set; } 

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DiseaseId")]
        public Disease? Disease { get; set; }
    }
}