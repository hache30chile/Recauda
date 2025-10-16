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

        public async Task<bool> EsUsuarioGenerador(int usuarioId)
        {
            _logger.LogInformation($"Verificando si usuario ID: {usuarioId} es generador");
            return await _context.Generadores
                .AnyAsync(g => g.usu_id == usuarioId && g.gen_activo);
        }

        public async Task CrearUsuarioAsync(Usuario usuario, bool esGenerador = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Iniciando creación de usuario: '{usuario.Login}' - Es generador: {esGenerador}");

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

                // Guardar cambios para obtener el ID del usuario
                _logger.LogInformation("Guardando usuario en la base de datos");
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    _logger.LogWarning("No se guardaron cambios en la base de datos");
                    throw new InvalidOperationException("No se pudo guardar el usuario en la base de datos");
                }

                _logger.LogInformation($"Usuario '{usuario.Login}' creado exitosamente con ID: {usuario.Id}");

                // Si es generador, crear el registro en la tabla Generadores
                if (esGenerador)
                {
                    _logger.LogInformation($"Creando registro de generador para usuario ID: {usuario.Id}");

                    var generador = new Generadores
                    {
                        usu_id = usuario.Id,
                        gen_activo = true,
                        com_id = 1, // Valor temporal, se puede modificar después según requerimientos
                        FechaRegistro = DateTime.Now
                    };

                    _context.Generadores.Add(generador);
                    var generadorResult = await _context.SaveChangesAsync();

                    if (generadorResult > 0)
                    {
                        _logger.LogInformation($"Registro de generador creado exitosamente con ID: {generador.Id}");
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo crear el registro de generador");
                        throw new InvalidOperationException("No se pudo crear el registro de generador");
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Transacción completada exitosamente para usuario '{usuario.Login}'");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al crear usuario '{usuario.Login}': {ex.Message}");
                throw;
            }
        }

        public async Task EditarUsuarioAsync(Usuario usuario, bool esGenerador = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Iniciando edición de usuario ID: {usuario.Id}, Login: '{usuario.Login}' - Es generador: {esGenerador}");

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

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"Usuario editado exitosamente. Filas afectadas: {result}");

                // **DEBUGGING: Log del parámetro recibido**
                _logger.LogInformation($"DEBUG: Parámetro esGenerador recibido: {esGenerador}");

                // Verificar si actualmente es generador
                var generadorExistente = await _context.Generadores
                    .FirstOrDefaultAsync(g => g.usu_id == usuario.Id);

                _logger.LogInformation($"DEBUG: Generador existente encontrado: {generadorExistente != null}");
                if (generadorExistente != null)
                {
                    _logger.LogInformation($"DEBUG: Generador existente - ID: {generadorExistente.Id}, Activo: {generadorExistente.gen_activo}");
                }

                if (esGenerador)
                {
                    _logger.LogInformation($"DEBUG: Usuario debe ser generador - procesando...");

                    if (generadorExistente == null)
                    {
                        // Crear nuevo registro de generador
                        _logger.LogInformation($"DEBUG: Creando nuevo registro de generador para usuario ID: {usuario.Id}");

                        var nuevoGenerador = new Generadores
                        {
                            usu_id = usuario.Id,
                            gen_activo = true,
                            com_id = 1, // Valor temporal
                            FechaRegistro = DateTime.Now
                        };

                        _context.Generadores.Add(nuevoGenerador);
                        var generadorResult = await _context.SaveChangesAsync();

                        _logger.LogInformation($"DEBUG: Resultado SaveChanges para generador: {generadorResult} filas afectadas");
                        _logger.LogInformation($"DEBUG: Nuevo registro de generador creado con ID: {nuevoGenerador.Id}");
                    }
                    else if (!generadorExistente.gen_activo)
                    {
                        // Reactivar generador existente
                        _logger.LogInformation($"DEBUG: Reactivando generador existente ID: {generadorExistente.Id}");
                        generadorExistente.gen_activo = true;
                        var reactivacionResult = await _context.SaveChangesAsync();
                        _logger.LogInformation($"DEBUG: Resultado reactivación: {reactivacionResult} filas afectadas");
                    }
                    else
                    {
                        _logger.LogInformation($"DEBUG: Generador ya existe y está activo - no se requiere acción");
                    }
                }
                else
                {
                    _logger.LogInformation($"DEBUG: Usuario NO debe ser generador");

                    if (generadorExistente != null && generadorExistente.gen_activo)
                    {
                        // Desactivar generador
                        _logger.LogInformation($"DEBUG: Desactivando generador ID: {generadorExistente.Id}");
                        generadorExistente.gen_activo = false;
                        var desactivacionResult = await _context.SaveChangesAsync();
                        _logger.LogInformation($"DEBUG: Resultado desactivación: {desactivacionResult} filas afectadas");
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"DEBUG: Transacción commiteada exitosamente");
                _logger.LogInformation($"Edición de usuario completada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al editar usuario ID: {usuario.Id} - Detalle: {ex.Message}");
                _logger.LogError($"DEBUG: StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task EliminarUsuarioAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Iniciando eliminación de usuario con ID: {id}");

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    _logger.LogInformation($"Usuario encontrado para eliminar: {usuario.Login} (ID: {usuario.Id})");

                    // Primero eliminar o desactivar registros de generador relacionados
                    var generadores = await _context.Generadores
                        .Where(g => g.usu_id == id)
                        .ToListAsync();

                    if (generadores.Any())
                    {
                        _logger.LogInformation($"Desactivando {generadores.Count} registros de generador relacionados");
                        generadores.ForEach(g => g.gen_activo = false);
                        await _context.SaveChangesAsync();
                    }

                    _context.Usuarios.Remove(usuario);
                    var result = await _context.SaveChangesAsync();

                    _logger.LogInformation($"Usuario eliminado exitosamente. Filas afectadas: {result}");
                }
                else
                {
                    _logger.LogWarning($"No se encontró usuario con ID: {id} para eliminar");
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