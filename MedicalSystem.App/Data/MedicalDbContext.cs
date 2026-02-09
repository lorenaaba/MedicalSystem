using Microsoft.EntityFrameworkCore;
using MedicalSystem.App.Models;

namespace MedicalSystem.App.Data
{
    public class MedicalDbContext : DbContext
    {
        public MedicalDbContext(DbContextOptions<MedicalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Disease> Diseases { get; set; } = null!;
        public DbSet<Medication> Medications { get; set; } = null!;
        public DbSet<SpecialistExam> SpecialistExams { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Oib)
                      .IsRequired();

                entity.HasIndex(p => p.Oib)
                      .IsUnique();

                entity.HasMany(p => p.Diseases)
                      .WithOne(d => d.Patient)
                      .HasForeignKey(d => d.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Medications)
                      .WithOne(m => m.Patient)
                      .HasForeignKey(m => m.PatientId);

                entity.HasMany(p => p.SpecialistExams)
                      .WithOne(e => e.Patient)
                      .HasForeignKey(e => e.PatientId);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.FirstName)
                      .IsRequired();

                entity.Property(d => d.LastName)
                      .IsRequired();

                entity.HasMany(d => d.SpecialistExams)
                      .WithOne(e => e.Doctor)
                      .HasForeignKey(e => e.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Disease>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Name)
                      .IsRequired();

                entity.HasOne(d => d.Patient)
                      .WithMany(p => p.Diseases)
                      .HasForeignKey(d => d.PatientId);
            });

            modelBuilder.Entity<Medication>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                      .IsRequired();

                entity.HasOne(m => m.Patient)
                      .WithMany(p => p.Medications)
                      .HasForeignKey(m => m.PatientId);

                entity.HasOne(m => m.Disease)
                      .WithMany()
                      .HasForeignKey(m => m.DiseaseId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SpecialistExam>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ExamType)
                      .HasConversion<string>()
                      .IsRequired();

                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.SpecialistExams)
                      .HasForeignKey(e => e.PatientId);

                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.SpecialistExams)
                      .HasForeignKey(e => e.DoctorId);
            });
        }

    }
}
