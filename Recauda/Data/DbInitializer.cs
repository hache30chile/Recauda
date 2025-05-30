using Recauda.Models;

namespace Recauda.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Crear rol administrador si no existe
            if (!context.Roles.Any())
            {
                var adminRole = new Rol
                {
                    Nombre = "Administrador"
                };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();

                // Crear usuario administrador
                var adminUser = new Usuario
                {
                    Login = "admin",
                    Rut = 12345678,
                    Dv = "5",
                    Nombre = "Administrador del Sistema",
                    Clave = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(12)),
                    RolId = adminRole.Id,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };
                context.Usuarios.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}