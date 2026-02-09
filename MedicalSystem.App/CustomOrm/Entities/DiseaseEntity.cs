using SimpleMedicalORM.Attributes;

namespace MedicalSystem.App.CustomOrm.Entities
{
    [Table("custom_diseases")]
    public class DiseaseEntity
    {
        [PrimaryKey(AutoIncrement = true)]
        [Column("disease_id")]
        public int DiseaseId { get; set; }

        [Column("patient_id", IsNullable = false)]
        public int PatientId { get; set; }

        [Column("name", SqlType = "VARCHAR(200)", IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [Column("diagnosis_date", IsNullable = false)]
        public DateTime DiagnosisDate { get; set; }

        [Column("recovery_date")]
        public DateTime? RecoveryDate { get; set; }

        [Column("notes", SqlType = "TEXT")]
        public string? Notes { get; set; }

        [NavigationProperty("PatientId")]
        public PatientEntity? Patient { get; set; }
    }
}