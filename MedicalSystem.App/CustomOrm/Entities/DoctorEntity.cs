using SimpleMedicalORM.Attributes;

namespace MedicalSystem.App.CustomOrm.Entities
{
    [Table("custom_doctors")]
    public class DoctorEntity
    {
        [PrimaryKey(AutoIncrement = true)]
        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("first_name", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string LastName { get; set; } = string.Empty;

        [Column("specialization", SqlType = "VARCHAR(100)", IsNullable = false)]
        public string Specialization { get; set; } = string.Empty;

        [Column("email", SqlType = "VARCHAR(100)", IsUnique = true)]
        public string? Email { get; set; }

        [Column("phone", SqlType = "VARCHAR(20)")]
        public string? Phone { get; set; }

        [Column("license_number", SqlType = "VARCHAR(50)", IsNullable = false, IsUnique = true)]
        public string LicenseNumber { get; set; } = string.Empty;
    }
}