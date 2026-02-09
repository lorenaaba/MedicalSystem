using SimpleMedicalORM.Attributes;

namespace MedicalSystem.App.CustomOrm.Entities
{
    [Table("custom_medications")]
    public class MedicationEntity
    {
        [PrimaryKey(AutoIncrement = true)]
        [Column("medication_id")]
        public int MedicationId { get; set; }

        [Column("patient_id", IsNullable = false)]
        public int PatientId { get; set; }

        [Column("name", SqlType = "VARCHAR(200)", IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [Column("dosage", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string Dosage { get; set; } = string.Empty;

        [Column("frequency", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string Frequency { get; set; } = string.Empty;

        [Column("start_date", IsNullable = false)]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("notes", SqlType = "TEXT")]
        public string? Notes { get; set; }
    }
}