using EcoSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);
                entity.HasIndex(user => user.Username).IsUnique();
                entity.Property(user => user.Username).HasMaxLength(50).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(120).IsRequired();
                entity.Property(user => user.PasswordHash).IsRequired();
                entity.Property(user => user.Role).HasMaxLength(30).IsRequired();
                entity.Property(user => user.CreatedAt).IsRequired();
            });
        }
    }
}
