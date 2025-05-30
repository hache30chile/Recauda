using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;

namespace Recauda.Controllers
{
    [Authorize]
    public class RolController : Controller
    {
        private readonly IRolService _rolService;
        private readonly ILogger<RolController> _logger;

        public RolController(IRolService rolService, ILogger<RolController> logger)
        {
            _rolService = rolService;
            _logger = logger;

        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _rolService.ObtenerRoles();
                return View(roles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los roles: " + ex.Message;
                return View(new List<Rol>());
            }
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rol rol)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe un rol con el mismo nombre
                    var rolesExistentes = await _rolService.ObtenerRoles();
                    if (rolesExistentes.Any(r => r.Nombre.ToLower() == rol.Nombre.ToLower()))
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un rol con este nombre.");
                        return View(rol);
                    }

                    await _rolService.CrearRolAsync(rol);

                    TempData["SuccessMessage"] = $"El rol '{rol.Nombre}' ha sido creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el rol: " + ex.Message;
            }

            return View(rol);
        }



        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var rol = await _rolService.ObtenerRolPorId(id);
                if (rol == null)
                {
                    TempData["ErrorMessage"] = "El rol solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(rol);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Rol rol)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe otro rol con el mismo nombre
                    var rolesExistentes = await _rolService.ObtenerRoles();
                    if (rolesExistentes.Any(r => r.Nombre.ToLower() == rol.Nombre.ToLower() && r.Id != rol.Id))
                    {
                        ModelState.AddModelError("Nombre", "Ya existe otro rol con este nombre.");
                        return View(rol);
                    }

                    await _rolService.EditarRolAsync(rol);

                    TempData["SuccessMessage"] = $"El rol '{rol.Nombre}' ha sido actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el rol: " + ex.Message;
            }

            return View(rol);
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var rol = await _rolService.ObtenerRolPorId(id);
                if (rol == null)
                {
                    TempData["ErrorMessage"] = "El rol solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(rol);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int id)
        {
            try
            {
                var rol = await _rolService.ObtenerRolPorId(id);
                if (rol == null)
                {
                    TempData["ErrorMessage"] = "El rol solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _rolService.EliminarRolAsync(id);
                TempData["SuccessMessage"] = $"El rol '{rol.Nombre}' ha sido eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el rol: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var rol = await _rolService.ObtenerRolPorId(id);
                if (rol == null)
                {
                    TempData["ErrorMessage"] = "El rol solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(rol);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}