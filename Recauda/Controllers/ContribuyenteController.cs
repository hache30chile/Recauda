using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;
using System.Security.Claims;

namespace Recauda.Controllers
{
    [Authorize]
    public class ContribuyenteController : Controller
    {
        private readonly IContribuyenteService _contribuyenteService;
        private readonly ILogger<ContribuyenteController> _logger;

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

                var rolNombre = User.FindFirstValue(ClaimTypes.Role);
                if (rolNombre?.ToLower() == "tesorero")
                {
                    var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var companiaId = await _contribuyenteService.ObtenerCompaniaDeUsuario(usuarioId);

                    if (companiaId.HasValue)
                    {
                        contribuyentes = contribuyentes
                            .Where(c => c.com_id == companiaId.Value)
                            .ToList();

                        ViewBag.FiltroCompania = true;
                        ViewBag.CompaniaNombre = contribuyentes.FirstOrDefault()?.com_nombre ?? string.Empty;
                    }
                    else
                    {
                        contribuyentes = new List<VContribuyente>();
                        TempData["ErrorMessage"] = "No tiene una compañía asignada. Contacte al administrador.";
                    }
                }

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

        public async Task<IActionResult> Crear()
        {
            try
            {
                await CargarViewBags();
                await CargarCompaniaFija();
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
                if (contribuyente.con_fecha_inicio == DateTime.MinValue)
                    contribuyente.con_fecha_inicio = DateTime.Now;

                if (contribuyente.con_fecha_fin.HasValue && contribuyente.con_fecha_fin.Value == DateTime.MinValue)
                    contribuyente.con_fecha_fin = null;

                contribuyente.FechaRegistro = DateTime.Now;

                if (string.IsNullOrEmpty(contribuyente.con_periodicidad_cobro))
                    contribuyente.con_periodicidad_cobro = "Mensual";

                // Si es Tesorero, forzar su propia compañía independiente de lo que venga en el form
                var rolNombre = User.FindFirstValue(ClaimTypes.Role);
                if (rolNombre?.ToLower() == "tesorero")
                {
                    var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var companiaId = await _contribuyenteService.ObtenerCompaniaDeUsuario(usuarioId);
                    if (companiaId.HasValue)
                        contribuyente.com_id = companiaId.Value;
                }

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
                _logger.LogError(ex, "Error al crear contribuyente");
                TempData["ErrorMessage"] = "Error al crear el contribuyente: " + ex.Message;
            }

            await CargarViewBags();
            await CargarCompaniaFija();
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
                await CargarCompaniaFija();
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
                // Si es Tesorero, forzar su propia compañía
                var rolNombre = User.FindFirstValue(ClaimTypes.Role);
                if (rolNombre?.ToLower() == "tesorero")
                {
                    var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var companiaId = await _contribuyenteService.ObtenerCompaniaDeUsuario(usuarioId);
                    if (companiaId.HasValue)
                        contribuyente.com_id = companiaId.Value;
                }

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
            await CargarCompaniaFija();
            return View(contribuyente);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPersona([FromBody] Persona persona)
        {
            try
            {
                if (string.IsNullOrEmpty(persona.per_nombres) || string.IsNullOrEmpty(persona.per_paterno))
                    return Json(new { success = false, message = "Nombres y apellido paterno son obligatorios" });

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

        // ── helpers ──────────────────────────────────────────────────────────

        private async Task CargarViewBags()
        {
            ViewBag.MotivosCobro = await _contribuyenteService.ObtenerMotivosCobro();
            ViewBag.Recaudadores = await _contribuyenteService.ObtenerRecaudadores();
            ViewBag.Companias = await _contribuyenteService.ObtenerCompanias();
        }

        /// <summary>
        /// Si el usuario autenticado es Tesorero, setea ViewBag.CompaniaIdFija y
        /// ViewBag.CompaniaNombreFija para que las vistas bloqueen el selector de compañía.
        /// </summary>
        private async Task CargarCompaniaFija()
        {
            var rolNombre = User.FindFirstValue(ClaimTypes.Role);
            if (rolNombre?.ToLower() != "tesorero") return;

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companiaId = await _contribuyenteService.ObtenerCompaniaDeUsuario(usuarioId);

            if (companiaId.HasValue)
            {
                var companias = await _contribuyenteService.ObtenerCompanias();
                var nombre = companias
                    .FirstOrDefault(c => c.Value == companiaId.Value.ToString())?.Text
                    ?? string.Empty;

                ViewBag.CompaniaIdFija = companiaId.Value;
                ViewBag.CompaniaNombreFija = nombre;
            }
        }
    }
}