using SimpleMedicalORM.Core;
using MedicalSystem.App.CustomOrm.Entities;

namespace MedicalSystem.App.CustomOrm.Context
{
    public class MedicalOrmContext : OrmContext
    {
        public MedicalOrmContext(string connectionString) : base(connectionString)
        {
        }

        public OrmSet<PatientEntity> Patients => Set<PatientEntity>();
        public OrmSet<DoctorEntity> Doctors => Set<DoctorEntity>();
        public OrmSet<DiseaseEntity> Diseases => Set<DiseaseEntity>();
        public OrmSet<MedicationEntity> Medications => Set<MedicationEntity>();

        protected override void InitializeSets()
        {
            var _ = Patients;
            var __ = Doctors;
            var ___ = Diseases;
            var ____ = Medications;

            Console.WriteLine("✅ All DbSets initialized");
        }
    }
}