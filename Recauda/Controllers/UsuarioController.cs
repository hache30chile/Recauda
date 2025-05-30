using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerUsuarios();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los usuarios: " + ex.Message;
                return View(new List<Usuario>());
            }
        }

        public async Task<IActionResult> Crear()
        {
            await CargarRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("Login,Rut,Dv,Nombre,Clave,RolId,Activo")] Usuario usuario)
        {
            _logger.LogInformation("POST Crear - Iniciando creación de usuario");
            _logger.LogInformation($"Datos recibidos - Login: '{usuario?.Login}', RUT: {usuario?.Rut}");

            // Limpiar errores de navegación del ModelState
            ModelState.Remove("Rol");

            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe un usuario con el mismo login o RUT
                    var usuariosExistentes = await _usuarioService.ObtenerUsuarios();
                    if (usuariosExistentes.Any(u => u.Login.ToLower() == usuario.Login.ToLower()))
                    {
                        ModelState.AddModelError("Login", "Ya existe un usuario con este login.");
                        await CargarRoles();
                        return View(usuario);
                    }

                    if (usuariosExistentes.Any(u => u.Rut == usuario.Rut))
                    {
                        ModelState.AddModelError("Rut", "Ya existe un usuario con este RUT.");
                        await CargarRoles();
                        return View(usuario);
                    }

                    await _usuarioService.CrearUsuarioAsync(usuario);

                    TempData["SuccessMessage"] = $"El usuario '{usuario.Login}' ha sido creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log de errores de validación
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"Error en {error.Key}:");
                        foreach (var subError in error.Value.Errors)
                        {
                            _logger.LogWarning($"  - {subError.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                TempData["ErrorMessage"] = "Error al crear el usuario: " + ex.Message;
            }

            await CargarRoles();
            return View(usuario);
        }

        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorId(id);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "El usuario solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await CargarRoles();
                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind("Id,Login,Rut,Dv,Nombre,RolId,Activo")] Usuario usuario)
        {
            _logger.LogInformation($"POST Editar - ID: {usuario.Id}, Login: '{usuario.Login}'");

            // Limpiar errores de navegación del ModelState
            ModelState.Remove("Rol");
            ModelState.Remove("Clave"); // No validar clave en edición

            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe otro usuario con el mismo login o RUT
                    var usuariosExistentes = await _usuarioService.ObtenerUsuarios();
                    if (usuariosExistentes.Any(u => u.Login.ToLower() == usuario.Login.ToLower() && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Login", "Ya existe otro usuario con este login.");
                        await CargarRoles();
                        return View(usuario);
                    }

                    if (usuariosExistentes.Any(u => u.Rut == usuario.Rut && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Rut", "Ya existe otro usuario con este RUT.");
                        await CargarRoles();
                        return View(usuario);
                    }

                    await _usuarioService.EditarUsuarioAsync(usuario);

                    TempData["SuccessMessage"] = $"El usuario '{usuario.Login}' ha sido actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log de errores de validación
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"Error en {error.Key}:");
                        foreach (var subError in error.Value.Errors)
                        {
                            _logger.LogWarning($"  - {subError.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario");
                TempData["ErrorMessage"] = "Error al actualizar el usuario: " + ex.Message;
            }

            await CargarRoles();
            return View(usuario);
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorId(id);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "El usuario solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorId(id);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "El usuario solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _usuarioService.EliminarUsuarioAsync(id);
                TempData["SuccessMessage"] = $"El usuario '{usuario.Login}' ha sido eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el usuario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorId(id);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "El usuario solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task CargarRoles()
        {
            var roles = await _usuarioService.ObtenerRolesActivos();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");
        }
    }
}