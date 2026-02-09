using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicalSystem.App.Data
{
    public class MedicalDbContextFactory : IDesignTimeDbContextFactory<MedicalDbContext>
    {
        public MedicalDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MedicalDbContext>();

            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=medicaldb;Username=medicaluser;Password=medicalpass123");

            return new MedicalDbContext(optionsBuilder.Options);
        }
    }
}
