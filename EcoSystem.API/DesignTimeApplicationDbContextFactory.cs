using EcoSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcoSystem.API
{
    public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\\\SQLEXPRESS;Database=EcoSystemDB;Trusted_Connection=True;TrustServerCertificate=True;", sqlOptions => sqlOptions.MigrationsAssembly("EcoSystem.API"));
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

