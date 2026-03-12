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
            var rolTesoreroId = await _usuarioService.ObtenerIdRolTesorero();
            await CargarRolesYCompanias(rolTesoreroId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [Bind("Login,Rut,Dv,Nombre,Clave,RolId,Activo")] Usuario usuario,
            bool esGenerador = false,
            int? companiaId = null)
        {
            _logger.LogInformation($"POST Crear - Login: '{usuario?.Login}', Es Generador: {esGenerador}, CompañíaId: {companiaId}");

            ModelState.Remove("Rol");

            var rolTesoreroId = await _usuarioService.ObtenerIdRolTesorero();

            // Validación de Tesorero: solo uno por compañía
            if (usuario.RolId == rolTesoreroId)
            {
                if (!companiaId.HasValue || companiaId == 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar una compañía para asignar el rol de Tesorero.");
                }
                else if (await _usuarioService.CompaniaTieneTesorero(companiaId.Value))
                {
                    ModelState.AddModelError("", "La compañía seleccionada ya tiene un Tesorero asignado. Solo puede haber un Tesorero por compañía.");
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var usuariosExistentes = await _usuarioService.ObtenerUsuarios();

                    if (usuariosExistentes.Any(u => u.Login.ToLower() == usuario.Login.ToLower()))
                    {
                        ModelState.AddModelError("Login", "Ya existe un usuario con este login.");
                        await CargarRolesYCompanias(rolTesoreroId);
                        ViewBag.CompaniaSeleccionada = companiaId;
                        return View(usuario);
                    }

                    if (usuariosExistentes.Any(u => u.Rut == usuario.Rut))
                    {
                        ModelState.AddModelError("Rut", "Ya existe un usuario con este RUT.");
                        await CargarRolesYCompanias(rolTesoreroId);
                        ViewBag.CompaniaSeleccionada = companiaId;
                        return View(usuario);
                    }

                    // El Tesorero siempre se registra como generador para guardar com_id
                    bool debeSerGenerador = esGenerador || usuario.RolId == rolTesoreroId;
                    await _usuarioService.CrearUsuarioAsync(usuario, debeSerGenerador, companiaId);

                    string mensaje = $"El usuario '{usuario.Login}' ha sido creado exitosamente";
                    if (usuario.RolId == rolTesoreroId)
                        mensaje += $" como Tesorero de la compañía seleccionada";
                    else if (esGenerador)
                        mensaje += " y configurado como generador";
                    mensaje += ".";

                    TempData["SuccessMessage"] = mensaje;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"Error en {error.Key}:");
                        foreach (var subError in error.Value.Errors)
                            _logger.LogWarning($"  - {subError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                TempData["ErrorMessage"] = "Error al crear el usuario: " + ex.Message;
            }

            await CargarRolesYCompanias(rolTesoreroId);
            ViewBag.CompaniaSeleccionada = companiaId;
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

                var rolTesoreroId = await _usuarioService.ObtenerIdRolTesorero();
                ViewBag.EsGenerador = await _usuarioService.EsUsuarioGenerador(id);
                ViewBag.RolTesoreroId = rolTesoreroId;
                ViewBag.CompaniaActual = await _usuarioService.ObtenerCompaniaDeTesorero(id);

                await CargarRolesYCompanias(rolTesoreroId);
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
        public async Task<IActionResult> Editar(
            [Bind("Id,Login,Rut,Dv,Nombre,RolId,Activo")] Usuario usuario,
            bool esGenerador = false,
            int? companiaId = null)
        {
            _logger.LogInformation($"POST Editar - ID: {usuario.Id}, Login: '{usuario.Login}', Es Generador: {esGenerador}, CompañíaId: {companiaId}");

            ModelState.Remove("Rol");
            ModelState.Remove("Clave");

            var rolTesoreroId = await _usuarioService.ObtenerIdRolTesorero();

            // Validación de Tesorero: solo uno por compañía
            if (usuario.RolId == rolTesoreroId)
            {
                if (!companiaId.HasValue || companiaId == 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar una compañía para asignar el rol de Tesorero.");
                }
                else if (await _usuarioService.CompaniaTieneTesorero(companiaId.Value, excludeUsuarioId: usuario.Id))
                {
                    ModelState.AddModelError("", "La compañía seleccionada ya tiene un Tesorero asignado. Solo puede haber un Tesorero por compañía.");
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var usuariosExistentes = await _usuarioService.ObtenerUsuarios();

                    if (usuariosExistentes.Any(u => u.Login.ToLower() == usuario.Login.ToLower() && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Login", "Ya existe otro usuario con este login.");
                        await CargarRolesYCompanias(rolTesoreroId);
                        ViewBag.EsGenerador = esGenerador;
                        ViewBag.CompaniaActual = companiaId;
                        return View(usuario);
                    }

                    if (usuariosExistentes.Any(u => u.Rut == usuario.Rut && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Rut", "Ya existe otro usuario con este RUT.");
                        await CargarRolesYCompanias(rolTesoreroId);
                        ViewBag.EsGenerador = esGenerador;
                        ViewBag.CompaniaActual = companiaId;
                        return View(usuario);
                    }

                    bool debeSerGenerador = esGenerador || usuario.RolId == rolTesoreroId;
                    await _usuarioService.EditarUsuarioAsync(usuario, debeSerGenerador, companiaId);

                    string mensaje = $"El usuario '{usuario.Login}' ha sido actualizado exitosamente";
                    if (usuario.RolId == rolTesoreroId)
                        mensaje += $" como Tesorero de la compañía seleccionada";
                    else if (esGenerador)
                        mensaje += " y configurado como generador";
                    mensaje += ".";

                    TempData["SuccessMessage"] = mensaje;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"Error en {error.Key}:");
                        foreach (var subError in error.Value.Errors)
                            _logger.LogWarning($"  - {subError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario");
                TempData["ErrorMessage"] = "Error al actualizar el usuario: " + ex.Message;
            }

            await CargarRolesYCompanias(rolTesoreroId);
            ViewBag.EsGenerador = esGenerador;
            ViewBag.RolTesoreroId = rolTesoreroId;
            ViewBag.CompaniaActual = companiaId;
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

                ViewBag.EsGenerador = await _usuarioService.EsUsuarioGenerador(id);
                ViewBag.CompaniaId = await _usuarioService.ObtenerCompaniaDeTesorero(id);

                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>Endpoint AJAX: verifica si una compañía ya tiene Tesorero asignado.</summary>
        [HttpGet]
        public async Task<IActionResult> VerificarTesorero(int companiaId, int? excludeUsuarioId = null)
        {
            try
            {
                bool tieneTes = await _usuarioService.CompaniaTieneTesorero(companiaId, excludeUsuarioId);
                return Json(new
                {
                    tieneTes,
                    mensaje = tieneTes
                        ? "Esta compañía ya tiene un Tesorero activo. No es posible asignar otro."
                        : string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar tesorero de compañía");
                return Json(new { tieneTes = false, mensaje = string.Empty });
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private async Task CargarRoles()
        {
            var roles = await _usuarioService.ObtenerRolesActivos();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");
        }

        private async Task CargarCompanias()
        {
            var companias = await _usuarioService.ObtenerCompanias();
            ViewBag.Companias = new SelectList(companias, "Id", "com_nombre");
        }

        private async Task CargarRolesYCompanias(int? rolTesoreroId)
        {
            await CargarRoles();
            await CargarCompanias();
            ViewBag.RolTesoreroId = rolTesoreroId;
        }
    }
}