using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystem.App.Models
{
    [Table("patients")]
    public class Patient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        [Column("oib")]
        public string Oib { get; set; } = string.Empty; 

        [Required]
        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(1)]
        [Column("gender")]
        public string Gender { get; set; } = string.Empty;

        [Required]  
        [MaxLength(200)]
        [Column("residence_address")]
        public string ResidenceAddress { get; set; } = string.Empty;

        [Required] 
        [MaxLength(200)]
        [Column("permanent_address")]
        public string PermanentAddress { get; set; } = string.Empty;

        public ICollection<Disease> Diseases { get; set; } = new List<Disease>();
        public ICollection<Medication> Medications { get; set; } = new List<Medication>();
        public ICollection<SpecialistExam> SpecialistExams { get; set; } = new List<SpecialistExam>();
    }
}