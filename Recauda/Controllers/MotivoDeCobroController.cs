using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;

namespace Recauda.Controllers
{
    [Authorize]
    public class MotivoDeCobroController : Controller
    {
        private readonly IMotivoDeCobroService _motivodecobroService;
        private readonly ILogger<MotivoDeCobroController> _logger;

        public MotivoDeCobroController(IMotivoDeCobroService motivodecobroService, ILogger<MotivoDeCobroController> logger)
        {
            _motivodecobroService = motivodecobroService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var motivodecobros = await _motivodecobroService.ObtenerMotivoDeCobros();
                return View(motivodecobros);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar Motivo de cobros: " + ex.Message;
                return View(new List<MotivoDeCobro>());
            }
        }

        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MotivoDeCobro motivodecobro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var motivodecobrosExistentes = await _motivodecobroService.ObtenerMotivoDeCobros();
                    if (motivodecobrosExistentes.Any(c => c.mdc_nombre.ToLower() == motivodecobro.mdc_nombre.ToLower()))
                    {
                        ModelState.AddModelError("mdc_nombre", "Ya existe.");
                        return View(motivodecobro);
                    }

                    await _motivodecobroService.CrearMotivoDeCobroAsync(motivodecobro);
                    TempData["SuccessMessage"] = $"El motivo de cobro '{motivodecobro.mdc_nombre}' ha sido creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear: " + ex.Message;
            }

            return View(motivodecobro);
        }


        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var motivodecobro = await _motivodecobroService.ObtenerMotivoDeCobroPorId(id);
                if (motivodecobro == null)
                {
                    TempData["ErrorMessage"] = $"La motivo de cobro con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(motivodecobro);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(MotivoDeCobro motivodecobro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var motivodecobrosExistentes = await _motivodecobroService.ObtenerMotivoDeCobros();
                    if (motivodecobrosExistentes.Any(c => c.mdc_nombre.ToLower() == motivodecobro.mdc_nombre.ToLower() && c.Id != motivodecobro.Id))
                    {
                        ModelState.AddModelError("mdc_nombre", "Ya existe este nombre.");
                        return View(motivodecobro);
                    }

                    await _motivodecobroService.EditarMotivoDeCobroAsync(motivodecobro);
                    TempData["SuccessMessage"] = $"MotivoDeCobro '{motivodecobro.mdc_nombre}' ha sido actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar: " + ex.Message;
            }

            return View(motivodecobro);
        }

 
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var motivodecobro = await _motivodecobroService.ObtenerMotivoDeCobroPorId(id);
                if (motivodecobro == null)
                {
                    TempData["ErrorMessage"] = $"El motivo de cobro con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(motivodecobro);
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
                var motivodecobro = await _motivodecobroService.ObtenerMotivoDeCobroPorId(id);
                if (motivodecobro == null)
                {
                    TempData["ErrorMessage"] = $"El motivo de cobro con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _motivodecobroService.EliminarMotivoDeCobroAsync(id);
                TempData["SuccessMessage"] = $"'{motivodecobro.mdc_nombre}' ha sido eliminada exitosamente.";
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
                var motivodecobro = await _motivodecobroService.ObtenerMotivoDeCobroPorId(id);
                if (motivodecobro == null)
                {
                    TempData["ErrorMessage"] = $"El motivo de cobro con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(motivodecobro);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}