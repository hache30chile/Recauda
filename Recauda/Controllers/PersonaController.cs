using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Interfaces;
using Recauda.Models;
using Recauda.Services;

namespace Recauda.Controllers
{
    [Authorize]

    // SE CAMBIO EL USO DE ID POR PER_RUT
    public class PersonaController : Controller
    {

        public readonly IPersonaService _personaService;
        public readonly ILogger<PersonaController> _logger;

        public PersonaController(IPersonaService personaService, ILogger<PersonaController> logger)
        {
            _personaService = personaService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var personas = await _personaService.ObtenerPersonas();
                return View(personas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las personas: " + ex.Message;
                return View(new List<Persona>());
            }
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Persona persona)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe una persona con este RUT
                    var personaExistente = await _personaService.ObtenerPersonaPorRut(persona.per_rut);
                    if (personaExistente != null)
                    {
                        ModelState.AddModelError("per_rut", "Ya existe una persona con este RUT.");
                        return View(persona);
                    }

                    await _personaService.CrearPersonaAsync(persona);

                    TempData["SuccessMessage"] = $"La persona con RUT '{persona.per_rut}' ha sido creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la persona: " + ex.Message;
            }

            return View(persona);
        }

        public async Task<IActionResult> Editar(int per_rut)
        {
            try
            {
                var persona = await _personaService.ObtenerPersonaPorRut(per_rut);
                if (persona == null)
                {
                    TempData["ErrorMessage"] = "La persona solicitada no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(persona);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la persona: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Persona persona)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _personaService.EditarPersonaAsync(persona);

                    TempData["SuccessMessage"] = $"La persona con RUT '{persona.per_rut}' ha sido actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la persona: " + ex.Message;
            }

            return View(persona);
        }

        public async Task<IActionResult> Eliminar(int per_rut)
        {
            try
            {
                var persona = await _personaService.ObtenerPersonaPorRut(per_rut);
                if (persona == null)
                {
                    TempData["ErrorMessage"] = "La persona solicitada no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(persona);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la persona: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int per_rut)
        {
            try
            {
                var persona = await _personaService.ObtenerPersonaPorRut(per_rut);
                if (persona == null)
                {
                    TempData["ErrorMessage"] = "La persona solicitada no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _personaService.EliminarPersonaAsync(per_rut);
                TempData["SuccessMessage"] = $"La persona con RUT '{persona.per_rut}' ha sido eliminada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la persona: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int per_rut)
        {
            try
            {
                var persona = await _personaService.ObtenerPersonaPorRut(per_rut);
                if (persona == null)
                {
                    TempData["ErrorMessage"] = "La persona solicitada no existe.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar campos críticos antes de pasar a la vista
                if (string.IsNullOrEmpty(persona.per_vrut) ||
                    string.IsNullOrEmpty(persona.per_paterno) ||
                    string.IsNullOrEmpty(persona.per_nombres))
                {
                    TempData["WarningMessage"] = $"Los datos de la persona RUT {persona.per_rut}-{persona.per_vrut} están incompletos.";
                }

                return View(persona);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la persona: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // MÉTODO ADICIONAL: Para mostrar el RUT formateado
        private string FormatearRut(int rut, string dv)
        {
            return $"{rut:N0}-{dv}";
        }
    }
}