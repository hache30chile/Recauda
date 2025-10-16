using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;

namespace Recauda.Controllers
{
    [Authorize]
    public class CompaniaController : Controller
    {
        private readonly ICompaniaService _companiaService;
        private readonly ILogger<CompaniaController> _logger;

        public CompaniaController(ICompaniaService companiaService, ILogger<CompaniaController> logger)
        {
            _companiaService = companiaService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var companias = await _companiaService.ObtenerCompanias();
                return View(companias);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar companias: " + ex.Message;
                return View(new List<Compania>());
            }
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Compania compania)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var companiasExistentes = await _companiaService.ObtenerCompanias();
                    if (companiasExistentes.Any(c => c.com_nombre.ToLower() == compania.com_nombre.ToLower()))
                    {
                        ModelState.AddModelError("Com_nombre", "Ya existe.");
                        return View(compania);
                    }

                    await _companiaService.CrearCompaniaAsync(compania);
                    TempData["SuccessMessage"] = $"La compania '{compania.com_nombre}' ha sido creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear: " + ex.Message;
            }

            return View(compania);
        }


        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var compania = await _companiaService.ObtenerCompaniaPorId(id);
                if (compania == null)
                {
                    TempData["ErrorMessage"] = $"La compañía con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(compania);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Compania compania)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var companiasExistentes = await _companiaService.ObtenerCompanias();
                    if (companiasExistentes.Any(c => c.com_nombre.ToLower() == compania.com_nombre.ToLower() && c.Id != compania.Id))
                    {
                        ModelState.AddModelError("Com_nombre", "Ya existe este nombre.");
                        return View(compania);
                    }

                    await _companiaService.EditarCompaniaAsync(compania);
                    TempData["SuccessMessage"] = $"Compania '{compania.com_nombre}' ha sido actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar: " + ex.Message;
            }

            return View(compania);
        }

 
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var compania = await _companiaService.ObtenerCompaniaPorId(id);
                if (compania == null)
                {
                    TempData["ErrorMessage"] = $"La compañía con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(compania);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ConfirmarEliminacion(int id)
        {
            try
            {
                var compania = await _companiaService.ObtenerCompaniaPorId(id);
                if (compania == null)
                {
                    TempData["ErrorMessage"] = $"La compañía con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _companiaService.EliminarCompaniaAsync(id);
                TempData["SuccessMessage"] = $"'{compania.com_nombre}' ha sido eliminada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var compania = await _companiaService.ObtenerCompaniaPorId(id);
                if (compania == null)
                {
                    TempData["ErrorMessage"] = $"La compañía con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(compania);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}