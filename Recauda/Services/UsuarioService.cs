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
                .Include(u => u.Rol)
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
                _logger.LogWarning($"No se encontró usuario con ID: {id}");

            return usuario;
        }

        public async Task<List<Rol>> ObtenerRolesActivos()
        {
            _logger.LogInformation("Obteniendo roles activos para dropdown");
            return await _context.Roles.ToListAsync();
        }

        public async Task<List<Compania>> ObtenerCompanias()
        {
            return await _context.Companias.OrderBy(c => c.com_nombre).ToListAsync();
        }

        public async Task<bool> EsUsuarioGenerador(int usuarioId)
        {
            _logger.LogInformation($"Verificando si usuario ID: {usuarioId} es generador");
            return await _context.Generadores
                .AnyAsync(g => g.usu_id == usuarioId && g.gen_activo);
        }

        /// <summary>Retorna el ID del rol cuyo nombre sea "Tesorero" (insensible a mayúsculas).</summary>
        public async Task<int?> ObtenerIdRolTesorero()
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre.ToLower() == "tesorero");
            return rol?.Id;
        }

        /// <summary>
        /// Indica si la compañía ya tiene un Tesorero activo asignado.
        /// Si se pasa <paramref name="excludeUsuarioId"/> se excluye ese usuario de la búsqueda
        /// (útil al editar para no bloquearse a sí mismo).
        /// </summary>
        public async Task<bool> CompaniaTieneTesorero(int companiaId, int? excludeUsuarioId = null)
        {
            var rolTesoreroId = await ObtenerIdRolTesorero();
            if (rolTesoreroId == null) return false;

            return await _context.Generadores
                .Include(g => g.Usuario)
                .AnyAsync(g =>
                    g.com_id == companiaId &&
                    g.gen_activo &&
                    g.Usuario != null &&
                    g.Usuario.RolId == rolTesoreroId &&
                    (excludeUsuarioId == null || g.usu_id != excludeUsuarioId));
        }

        /// <summary>Retorna el com_id del generador activo de un usuario, si existe.</summary>
        public async Task<int?> ObtenerCompaniaDeTesorero(int usuarioId)
        {
            var generador = await _context.Generadores
                .FirstOrDefaultAsync(g => g.usu_id == usuarioId && g.gen_activo);
            return generador?.com_id;
        }

        public async Task CrearUsuarioAsync(Usuario usuario, bool esGenerador = false, int? companiaId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Iniciando creación de usuario: '{usuario.Login}' - Es generador: {esGenerador}");

                usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave, BCrypt.Net.BCrypt.GenerateSalt(12));
                usuario.FechaRegistro = DateTime.Now;

                _context.Usuarios.Add(usuario);
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                    throw new InvalidOperationException("No se pudo guardar el usuario en la base de datos");

                _logger.LogInformation($"Usuario '{usuario.Login}' creado con ID: {usuario.Id}");

                if (esGenerador)
                {
                    var generador = new Generadores
                    {
                        usu_id = usuario.Id,
                        gen_activo = true,
                        com_id = companiaId ?? 1,
                        FechaRegistro = DateTime.Now
                    };

                    _context.Generadores.Add(generador);
                    var genResult = await _context.SaveChangesAsync();

                    if (genResult == 0)
                        throw new InvalidOperationException("No se pudo crear el registro de generador");

                    _logger.LogInformation($"Generador creado con ID: {generador.Id}, com_id: {generador.com_id}");
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Transacción completada para usuario '{usuario.Login}'");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al crear usuario '{usuario.Login}': {ex.Message}");
                throw;
            }
        }

        public async Task EditarUsuarioAsync(Usuario usuario, bool esGenerador = false, int? companiaId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Editando usuario ID: {usuario.Id}, Login: '{usuario.Login}' - Es generador: {esGenerador}");

                var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id)
                    ?? throw new InvalidOperationException($"No se encontró el usuario con ID: {usuario.Id}");

                existingUsuario.Login = usuario.Login;
                existingUsuario.Rut = usuario.Rut;
                existingUsuario.Dv = usuario.Dv;
                existingUsuario.Nombre = usuario.Nombre;
                existingUsuario.RolId = usuario.RolId;
                existingUsuario.Activo = usuario.Activo;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Datos base del usuario actualizados.");

                var generadorExistente = await _context.Generadores
                    .FirstOrDefaultAsync(g => g.usu_id == usuario.Id);

                if (esGenerador)
                {
                    if (generadorExistente == null)
                    {
                        var nuevoGenerador = new Generadores
                        {
                            usu_id = usuario.Id,
                            gen_activo = true,
                            com_id = companiaId ?? 1,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Generadores.Add(nuevoGenerador);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Nuevo generador creado con ID: {nuevoGenerador.Id}");
                    }
                    else
                    {
                        // Actualizar estado activo y compañía si se proporcionó
                        generadorExistente.gen_activo = true;
                        if (companiaId.HasValue && companiaId.Value > 0)
                            generadorExistente.com_id = companiaId.Value;

                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Generador existente actualizado.");
                    }
                }
                else
                {
                    if (generadorExistente != null && generadorExistente.gen_activo)
                    {
                        generadorExistente.gen_activo = false;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Generador desactivado.");
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Edición completada para usuario '{usuario.Login}'");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al editar usuario ID: {usuario.Id}: {ex.Message}");
                throw;
            }
        }

        public async Task EliminarUsuarioAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Eliminando usuario con ID: {id}");

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    var generadores = await _context.Generadores
                        .Where(g => g.usu_id == id)
                        .ToListAsync();

                    if (generadores.Any())
                    {
                        generadores.ForEach(g => g.gen_activo = false);
                        await _context.SaveChangesAsync();
                    }

                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Usuario '{usuario.Login}' eliminado.");
                }
                else
                {
                    _logger.LogWarning($"No se encontró usuario con ID: {id}");
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al eliminar usuario con ID: {id}");
                throw;
            }
        }
    }
}