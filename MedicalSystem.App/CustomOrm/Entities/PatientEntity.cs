using SimpleMedicalORM.Attributes;

namespace MedicalSystem.App.CustomOrm.Entities
{
    [Table("custom_patients")]
    public class PatientEntity
    {
        [PrimaryKey(AutoIncrement = true)]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("first_name", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string LastName { get; set; } = string.Empty;

        [Column("oib", SqlType = "VARCHAR(11)", IsNullable = false, IsUnique = true)]
        public string OIB { get; set; } = string.Empty;

        [Column("date_of_birth", IsNullable = false)]
        public DateTime DateOfBirth { get; set; }

        [Column("gender", SqlType = "VARCHAR(1)", IsNullable = false)]
        public string Gender { get; set; } = string.Empty;

        [Column("phone", SqlType = "VARCHAR(20)")]
        public string? Phone { get; set; }

        [Column("email", SqlType = "VARCHAR(100)")]
        public string? Email { get; set; }

        [Column("address", SqlType = "TEXT")]
        public string? Address { get; set; }

        [NavigationProperty("PatientId")]
        public List<DiseaseEntity> Diseases { get; set; } = new();
    }
}