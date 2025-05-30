using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(AppDbContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Usuario>> ObtenerUsuarios()
        {
            _logger.LogInformation("Obteniendo todos los usuarios con sus roles");
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol) // Incluir el rol relacionado
                .ToListAsync();
            _logger.LogInformation($"Se encontraron {usuarios.Count} usuarios");
            return usuarios;
        }

        public async Task<Usuario?> ObtenerUsuarioPorId(int id)
        {
            _logger.LogInformation($"Buscando usuario con ID: {id}");
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                _logger.LogWarning($"No se encontró usuario con ID: {id}");
            }
            return usuario;
        }

        public async Task<List<Rol>> ObtenerRolesActivos()
        {
            _logger.LogInformation("Obteniendo roles activos para dropdown");
            return await _context.Roles.ToListAsync();
        }

        public async Task CrearUsuarioAsync(Usuario usuario)
        {
            try
            {
                _logger.LogInformation($"Iniciando creación de usuario: '{usuario.Login}'");

                // Verificar que el contexto esté disponible
                if (_context == null)
                {
                    _logger.LogError("El contexto de base de datos es null");
                    throw new InvalidOperationException("El contexto de base de datos no está disponible");
                }

                // Hashear la contraseña antes de guardar
                usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave, BCrypt.Net.BCrypt.GenerateSalt(12));

                // Establecer fecha de registro
                usuario.FechaRegistro = DateTime.Now;

                // Agregar el usuario
                _logger.LogInformation("Agregando usuario al contexto");
                _context.Usuarios.Add(usuario);

                // Guardar cambios
                _logger.LogInformation("Guardando cambios en la base de datos");
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"Cambios guardados. Filas afectadas: {result}");

                if (result == 0)
                {
                    _logger.LogWarning("No se guardaron cambios en la base de datos");
                    throw new InvalidOperationException("No se pudo guardar el usuario en la base de datos");
                }

                _logger.LogInformation($"Usuario '{usuario.Login}' creado exitosamente con ID: {usuario.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear usuario '{usuario.Login}': {ex.Message}");
                throw;
            }
        }

        public async Task EditarUsuarioAsync(Usuario usuario)
        {
            try
            {
                _logger.LogInformation($"Iniciando edición de usuario ID: {usuario.Id}, Login: '{usuario.Login}'");

                // Método más seguro: Buscar la entidad existente y actualizar sus propiedades
                var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id);

                if (existingUsuario == null)
                {
                    _logger.LogError($"No se encontró usuario con ID: {usuario.Id}");
                    throw new InvalidOperationException($"No se encontró el usuario con ID: {usuario.Id}");
                }

                // Actualizar solo las propiedades que queremos cambiar
                existingUsuario.Login = usuario.Login;
                existingUsuario.Rut = usuario.Rut;
                existingUsuario.Dv = usuario.Dv;
                existingUsuario.Nombre = usuario.Nombre;
                existingUsuario.RolId = usuario.RolId;
                existingUsuario.Activo = usuario.Activo;
                // NO actualizar la clave aquí a menos que sea específicamente requerido
                // existingUsuario.Clave = usuario.Clave;

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"Usuario editado exitosamente. Filas afectadas: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar usuario ID: {usuario.Id}");
                throw;
            }
        }

        public async Task EliminarUsuarioAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Iniciando eliminación de usuario con ID: {id}");

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    _logger.LogInformation($"Usuario encontrado para eliminar: {usuario.Login} (ID: {usuario.Id})");

                    _context.Usuarios.Remove(usuario);
                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation($"Usuario eliminado exitosamente. Filas afectadas: {result}");
                }
                else
                {
                    _logger.LogWarning($"No se encontró usuario con ID: {id} para eliminar");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario con ID: {id}");
                throw;
            }
        }
    }
}