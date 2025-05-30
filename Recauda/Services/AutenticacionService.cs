using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Recauda.Services
{
    public class AutenticacionService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AutenticacionService> _logger;

        public AutenticacionService(AppDbContext context, ILogger<AutenticacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Usuario?> ValidarCredencialesAsync(string login, string clave)
        {
            try
            {
                _logger.LogInformation($"Intentando validar credenciales para usuario: {login}");

                // Buscar usuario activo con su rol
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower() && u.Activo);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado o inactivo: {login}");
                    return null;
                }

                // Verificar contraseña
                bool claveValida = VerificarClave(clave, usuario.Clave);

                if (!claveValida)
                {
                    _logger.LogWarning($"Contraseña incorrecta para usuario: {login}");
                    return null;
                }

                _logger.LogInformation($"Login exitoso para usuario: {login}");
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar credenciales para usuario: {login}");
                return null;
            }
        }

        public async Task<bool> CambiarClaveAsync(int usuarioId, string claveActual, string claveNueva)
        {
            try
            {
                _logger.LogInformation($"Intentando cambiar contraseña para usuario ID: {usuarioId}");

                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado para cambio de contraseña: {usuarioId}");
                    return false;
                }

                // Verificar contraseña actual
                if (!VerificarClave(claveActual, usuario.Clave))
                {
                    _logger.LogWarning($"Contraseña actual incorrecta para usuario: {usuarioId}");
                    return false;
                }

                // Hashear nueva contraseña
                usuario.Clave = HashearClave(claveNueva);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña cambiada exitosamente para usuario: {usuarioId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar contraseña para usuario: {usuarioId}");
                return false;
            }
        }

        public string HashearClave(string clave)
        {
            // Usar BCrypt para hashear contraseñas de forma segura
            return BCrypt.Net.BCrypt.HashPassword(clave, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerificarClave(string clave, string hashAlmacenado)
        {
            try
            {
                if (!hashAlmacenado.StartsWith("$2"))
                {
                    _logger.LogWarning("Contraseña sin hashear detectada - considerar migración");
                    return clave == hashAlmacenado;
                }

                // Verificar con BCrypt
                return BCrypt.Net.BCrypt.Verify(clave, hashAlmacenado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña");
                return false;
            }
        }
    }
}