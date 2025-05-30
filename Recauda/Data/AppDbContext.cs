using Microsoft.EntityFrameworkCore;
using Recauda.Models;

namespace Recauda.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.Property(e => e.Nombre)
                    .IsRequired()             // obligatorio
                    .HasMaxLength(50)         // longitud máxima 50
                    .HasColumnType("varchar(50)"); // tipo varchar(50) en la BD
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Login)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Rut)
                      .IsRequired();

                entity.Property(e => e.Dv)
                      .IsRequired()
                      .HasMaxLength(1);

                entity.Property(e => e.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Clave)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                entity.Property(e => e.Activo)
                      .IsRequired();

                // Relación explícita entre Usuario y Rol
                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.RolId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
