using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;

namespace Recauda.Controllers
{
    [Authorize]
        
    public class ContribuyenteController : Controller
    {

        private readonly IContribuyenteService _contribuyenteService;
        private readonly ILogger<ContribuyenteController> _logger;
        private object _context;

        public ContribuyenteController(IContribuyenteService contribuyenteService, ILogger<ContribuyenteController> logger)
        {
            _contribuyenteService = contribuyenteService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var contribuyentes = await _contribuyenteService.ObtenerContribuyentes();
                return View(contribuyentes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los contribuyentes: " + ex.Message;
                return View(new List<VContribuyente>());
            }
        }
        public async Task<IActionResult> Eliminar(int Id)
        {
            try
            {
                var contribuyente = await _contribuyenteService.ObtenerContribuyentePorId(Id);
                if (contribuyente == null)
                {
                    TempData["ErrorMessage"] = "El contribuyente solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(contribuyente);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el contribuyente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int Id)
        {
            try
            {
                var contribuyente = await _contribuyenteService.ObtenerContribuyentePorId(Id);
                if (contribuyente == null)
                {
                    TempData["ErrorMessage"] = "El contribuyente solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _contribuyenteService.EliminarContribuyenteAsync(Id);
                TempData["SuccessMessage"] = $"El contribuyente con ID '{contribuyente.Id}' ha sido eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el contribuyente: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int Id)
        {
            try
            {
                var contribuyente = await _contribuyenteService.ObtenerContribuyentePorId(Id);
                if (contribuyente == null)
                {
                    TempData["ErrorMessage"] = "El contribuyente solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                return View(contribuyente);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la contribuyente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Método GET para mostrar el formulario de crear
        public async Task<IActionResult> Crear()
        {
            try
            {
                // Cargar datos para los dropdowns
                await CargarViewBags();
                return View(new Contribuyente());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("per_rut,mdc_id,con_valor_aporte,con_periodicidad_cobro,con_dia_del_cargo,con_fecha_inicio,con_fecha_fin,rec_id,con_activo,com_id")] Contribuyente contribuyente)
        {
            _logger.LogInformation("POST Crear - Iniciando creación de contribuyente");

            try
            {
                // Validar fechas antes de procesar
                if (contribuyente.con_fecha_inicio == DateTime.MinValue)
                {
                    contribuyente.con_fecha_inicio = DateTime.Now;
                }

                // Si con_fecha_fin está vacía o es MinValue, establecerla como null
                if (contribuyente.con_fecha_fin.HasValue && contribuyente.con_fecha_fin.Value == DateTime.MinValue)
                {
                    contribuyente.con_fecha_fin = null;
                }

                // Establecer fecha de registro
                contribuyente.FechaRegistro = DateTime.Now;

                // Establecer periodicidad por defecto si no viene
                if (string.IsNullOrEmpty(contribuyente.con_periodicidad_cobro))
                {
                    contribuyente.con_periodicidad_cobro = "Mensual";
                }

                // Limpiar errores de navegación del ModelState
                ModelState.Remove("Persona");
                ModelState.Remove("MotivoCobro");
                ModelState.Remove("Recaudador");
                ModelState.Remove("Compania");
                ModelState.Remove("Cobros");

                if (ModelState.IsValid)
                {
                    await _contribuyenteService.CrearContribuyenteAsync(contribuyente);
                    TempData["SuccessMessage"] = $"El contribuyente ha sido creado exitosamente.";
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
                _logger.LogError(ex, "Error al crear contribuyente");
                TempData["ErrorMessage"] = "Error al crear el contribuyente: " + ex.Message;
            }

            await CargarViewBags();
            return View(contribuyente);
        }

        public async Task<IActionResult> Editar(int Id)
        {
            try
            {
                var contribuyente = await _contribuyenteService.ObtenerContribuyentePorId(Id);
                if (contribuyente == null)
                {
                    TempData["ErrorMessage"] = "El contribuyente solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                var contribuyenteModel = new Contribuyente
                {
                    Id = contribuyente.Id,
                    per_rut = contribuyente.per_rut,
                    mdc_id = contribuyente.mdc_id,
                    con_valor_aporte = contribuyente.con_valor_aporte,
                    con_periodicidad_cobro = contribuyente.con_periodicidad_cobro,
                    con_dia_del_cargo = contribuyente.con_dia_del_cargo,
                    con_fecha_inicio = contribuyente.con_fecha_inicio,
                    con_fecha_fin = contribuyente.con_fecha_fin,
                    rec_id = contribuyente.rec_id,
                    con_activo = contribuyente.con_activo,
                    com_id = contribuyente.com_id,
                    FechaRegistro = contribuyente.FechaRegistro
                };

                await CargarViewBags();
                return View(contribuyenteModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el contribuyente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Contribuyente contribuyente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _contribuyenteService.EditarContribuyenteAsync(contribuyente);
                    TempData["SuccessMessage"] = $"El contribuyente con ID '{contribuyente.Id}' ha sido actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el contribuyente: " + ex.Message;
            }

            await CargarViewBags();
            return View(contribuyente);
        }

        // Método AJAX para buscar persona por RUT
        [HttpGet]
        public async Task<IActionResult> BuscarPersona(int rut)
        {
            try
            {
                var persona = await _contribuyenteService.BuscarPersonaPorRut(rut);

                if (persona != null)
                {
                    return Json(new
                    {
                        success = true,
                        persona = new
                        {
                            nombreCompleto = $"{persona.per_nombres} {persona.per_paterno} {persona.per_materno}".Trim()
                        }
                    });
                }
                else
                {
                    return Json(new { success = true, persona = (object)null });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método AJAX para crear nueva persona
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPersona([FromBody] Persona persona)
        {
            try
            {
                if (string.IsNullOrEmpty(persona.per_nombres) || string.IsNullOrEmpty(persona.per_paterno))
                {
                    return Json(new { success = false, message = "Nombres y apellido paterno son obligatorios" });
                }

                await _contribuyenteService.CrearPersonaAsync(persona);

                return Json(new
                {
                    success = true,
                    nombreCompleto = $"{persona.per_nombres} {persona.per_paterno} {persona.per_materno}".Trim()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método privado para cargar los datos de los dropdowns
        private async Task CargarViewBags()
        {
            ViewBag.MotivosCobro = await _contribuyenteService.ObtenerMotivosCobro();
            ViewBag.Recaudadores = await _contribuyenteService.ObtenerRecaudadores();
            ViewBag.Companias = await _contribuyenteService.ObtenerCompanias();
        }

    }
}