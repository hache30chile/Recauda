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

    public class RecaudadorController : Controller
    {

        private readonly IRecaudadorService _recaudadorService;
        private readonly ILogger<RecaudadorController> _logger;
        private readonly ComprobantePagoService _comprobantePagoService; 
        private object _context;


        public RecaudadorController(IRecaudadorService recaudadorService, ILogger<RecaudadorController> logger, ComprobantePagoService comprobantePagoService)
        {
            _recaudadorService = recaudadorService;
            _logger = logger;
            _comprobantePagoService = comprobantePagoService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var recaudadores = await _recaudadorService.ObtenerRecaudadores();
                return View(recaudadores);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los recaudadores: " + ex.Message;
                return View(new List<VRecaudador>());
            }
        }


        public async Task<IActionResult> Crear()
        {
            await CargarUsuarios();
            await CargarCompanias();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("rec_activo,usu_id,com_id")] Recaudador recaudador)
        {
            _logger.LogInformation("POST Crear - Iniciando creación de recaudador");
            //  _logger.LogInformation($"Datos recibidos - Login: '{usuario?.Login}', RUT: {usuario?.Rut}");
            // Limpiar errores de navegación del ModelState
            ModelState.Remove("Recaudador");
            try
            {
                if (ModelState.IsValid)
                {

                    var recaudadorExistente = await _recaudadorService.ObtenerRecaudadorPorId(recaudador.Id);
                    if (recaudadorExistente != null)
                    {
                        ModelState.AddModelError("Id", "Ya existe un recaudador con este Id.");
                        await CargarUsuarios();
                        await CargarCompanias();
                        return View(recaudador);
                    }
                    await _recaudadorService.CrearRecaudadorAsync(recaudador);
                    TempData["SuccessMessage"] = $"El recaudador con ID '{recaudador.Id}' ha sido creado exitosamente.";
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
                TempData["ErrorMessage"] = "Error al crear el recaudador: " + ex.Message;
            }
            await CargarUsuarios();
            await CargarCompanias();
            return View(recaudador);
        }


        public async Task<IActionResult> Editar(int Id)
        {
            try
            {
                var recaudador = await _recaudadorService.ObtenerRecaudadorPorId(Id);
                if (recaudador == null)
                {
                    TempData["ErrorMessage"] = "El recaudador solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                await CargarUsuarios();
                await CargarCompanias();
                return View(recaudador);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el recaudador: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Recaudador recaudador)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _recaudadorService.EditarRecaudadorAsync(recaudador);

                    TempData["SuccessMessage"] = $"El recaudador con ID '{recaudador.Id}' ha sido actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el recaudador: " + ex.Message;
            }
            await CargarUsuarios();
            await CargarCompanias();
            return View(recaudador);
        }


        public async Task<IActionResult> Eliminar(int Id)
        {
            try
            {
                var recaudadores = await _recaudadorService.ObtenerRecaudadores();
                var recaudador = recaudadores.FirstOrDefault(r => r.Id == Id);

                if (recaudador == null)
                {
                    TempData["ErrorMessage"] = "El recaudador solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                return View(recaudador);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el recaudador: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int Id)
        {
            try
            {
                var recaudador = await _recaudadorService.ObtenerRecaudadorPorId(Id);
                if (recaudador == null)
                {
                    TempData["ErrorMessage"] = "El recaudador solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                await _recaudadorService.EliminarRecaudadorAsync(Id);
                TempData["SuccessMessage"] = $"El recaudador con ID '{recaudador.Id}' ha sido eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el recaudador: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Detalles(int Id)
        {
            try
            {
                var recaudador = await _recaudadorService.ObtenerRecaudadorPorId(Id);
                if (recaudador == null)
                {
                    TempData["ErrorMessage"] = "El recaudador solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                return View(recaudador);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la recaudador: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        private async Task CargarUsuarios()
        {
            var usuarios = await _recaudadorService.ObtenerUsuariosActivos();
            if (usuarios != null)
            {
                ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre");
            }

        }

        private async Task CargarCompanias()
        {
            var companias = await _recaudadorService.ObtenerCompaniasActivos();
            ViewBag.Companias = new SelectList(companias, "Id", "com_nombre");
        }

        /// Obtiene el ID del usuario actual logueado
        private int ObtenerUsuarioActualId()
        {
            try
            {
                // Opción 1: Si guardas el ID del usuario en los Claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }

                // Opción 2: Si guardas el ID en un claim personalizado
                var customUserIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(customUserIdClaim, out int customUserId))
                {
                    return customUserId;
                }

                // Opción 3: Si guardas el ID en un claim con nombre "Id"
                var idClaim = User.FindFirst("Id")?.Value;
                if (int.TryParse(idClaim, out int id))
                {
                    return id;
                }

                _logger.LogWarning("No se pudo obtener el ID del usuario actual de los claims");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ID del usuario actual");
                return 0;
            }
        }

        /// Muestra la vista para generar cobros
        public async Task<IActionResult> GenerarCobros(string? rutBusqueda = null)
        {
            await CargarContribuyentes();
            ViewBag.AnioActual = DateTime.Now.Year;

            // Cargar cobros
            List<VCobros> cobros;
            if (!string.IsNullOrWhiteSpace(rutBusqueda))
            {
                var rutFormateado = RecaudadorService.FormatearRutParaBusqueda(rutBusqueda);
                if (rutFormateado.HasValue)
                {
                    cobros = await _recaudadorService.BuscarCobrosPorRut(rutFormateado.Value);
                    ViewBag.RutBusqueda = rutBusqueda;

                    if (cobros.Count == 0)
                    {
                        TempData["InfoMessage"] = $"No se encontraron cobros para el RUT: {rutBusqueda}";
                    }
                }
                else
                {
                    cobros = new List<VCobros>();
                    ViewBag.RutBusqueda = rutBusqueda;
                    TempData["ErrorMessage"] = $"El RUT ingresado '{rutBusqueda}' no tiene un formato válido.";
                }
            }
            else
            {
                cobros = await _recaudadorService.ObtenerTodosLosCobros();
            }

            ViewBag.Cobros = cobros;
            return View();
        }

        /// Procesa la generación de cobros
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarCobros(int anio, int contribuyenteId, string? rutBusqueda = null)
        {
            _logger.LogInformation("POST GenerarCobros - Iniciando generación de cobros");
            _logger.LogInformation($"Datos recibidos - Año: {anio}, Contribuyente ID: {contribuyenteId}");

            try
            {
                // Obtener el ID del usuario actual
                var usuarioActualId = ObtenerUsuarioActualId();
                if (usuarioActualId == 0)
                {
                    TempData["ErrorMessage"] = "No se pudo identificar al usuario actual. Por favor, inicie sesión nuevamente.";
                    await CargarDatosVista(anio, contribuyenteId, rutBusqueda);
                    return View();
                }

                // Validaciones
                if (anio < 2020 || anio > DateTime.Now.Year + 5)
                {
                    ModelState.AddModelError("anio", "El año debe estar entre 2020 y " + (DateTime.Now.Year + 5));
                }

                if (contribuyenteId <= 0)
                {
                    ModelState.AddModelError("contribuyenteId", "Debe seleccionar un contribuyente válido.");
                }

                if (ModelState.IsValid)
                {
                    // Obtener el contribuyente
                    var contribuyente = await _recaudadorService.ObtenerContribuyentePorId(contribuyenteId);
                    if (contribuyente == null)
                    {
                        ModelState.AddModelError("contribuyenteId", "El contribuyente seleccionado no existe.");
                        await CargarDatosVista(anio, contribuyenteId, rutBusqueda);
                        return View();
                    }

                    // Verificar si el contribuyente está activo
                    if (!contribuyente.con_activo)
                    {
                        ModelState.AddModelError("contribuyenteId", "El contribuyente seleccionado no está activo.");
                        await CargarDatosVista(anio, contribuyenteId, rutBusqueda);
                        return View();
                    }

                    // Generar los cobros para el año seleccionado
                    var cobrosGenerados = await _recaudadorService.GenerarCobrosAsync(contribuyente, anio, usuarioActualId);

                    if (cobrosGenerados > 0)
                    {
                        TempData["SuccessMessage"] = $"Se han generado {cobrosGenerados} cobro(s) exitosamente para el contribuyente {contribuyente.per_nombre_completo} en el año {anio}.";
                    }
                    else
                    {
                        TempData["InfoMessage"] = "No se generaron cobros. Es posible que ya existan cobros para este período o que las fechas no sean válidas.";
                    }

                    return RedirectToAction("GenerarCobros", new { rutBusqueda = rutBusqueda });
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
                _logger.LogError(ex, "Error al generar cobros");
                TempData["ErrorMessage"] = "Error al generar los cobros: " + ex.Message;
            }

            await CargarDatosVista(anio, contribuyenteId, rutBusqueda);
            return View();
        }

        /// Carga la lista de contribuyentes activos para el dropdown
        private async Task CargarContribuyentes()
        {
            var contribuyentes = await _recaudadorService.ObtenerContribuyentesActivos();
            if (contribuyentes != null)
            {
                ViewBag.Contribuyentes = new SelectList(contribuyentes, "Id", "per_nombre_completo");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarCobrosPorRut(string rutBusqueda)
        {
            return RedirectToAction("GenerarCobros", new { rutBusqueda = rutBusqueda });
        }

        /// Método auxiliar para cargar todos los datos de la vista
        private async Task CargarDatosVista(int? anio = null, int? contribuyenteId = null, string? rutBusqueda = null)
        {
            await CargarContribuyentes();
            ViewBag.AnioActual = DateTime.Now.Year;

            if (anio.HasValue)
                ViewBag.AnioSeleccionado = anio.Value;

            if (contribuyenteId.HasValue)
                ViewBag.ContribuyenteSeleccionado = contribuyenteId.Value;

            // Cargar cobros según el filtro de RUT
            List<VCobros> cobros;
            if (!string.IsNullOrWhiteSpace(rutBusqueda))
            {
                var rutFormateado = RecaudadorService.FormatearRutParaBusqueda(rutBusqueda);
                if (rutFormateado.HasValue)
                {
                    cobros = await _recaudadorService.BuscarCobrosPorRut(rutFormateado.Value);
                    ViewBag.RutBusqueda = rutBusqueda;
                }
                else
                {
                    cobros = new List<VCobros>();
                    ViewBag.RutBusqueda = rutBusqueda;
                    TempData["ErrorMessage"] = $"El RUT ingresado '{rutBusqueda}' no tiene un formato válido.";
                }
            }
            else
            {
                cobros = await _recaudadorService.ObtenerTodosLosCobros();
            }

            ViewBag.Cobros = cobros;
        }

        /// Muestra la vista de pagos
        public async Task<IActionResult> Pagos(int? contribuyenteId = null)
        {
            await CargarContribuyentes();

            List<VCobros> cobros = new List<VCobros>();

            if (contribuyenteId.HasValue && contribuyenteId.Value > 0)
            {
                cobros = await _recaudadorService.ObtenerCobrosPorContribuyente(contribuyenteId.Value);
                ViewBag.ContribuyenteSeleccionado = contribuyenteId.Value;

                if (cobros.Count == 0)
                {
                    TempData["InfoMessage"] = "El contribuyente seleccionado no tiene cobros registrados.";
                }
            }

            ViewBag.Cobros = cobros;
            return View();
        }

        /// Procesa la selección de contribuyente para mostrar sus cobros
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagos(int contribuyenteId)
        {
            return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
        }

        /// Muestra el modal/vista para registrar un pago
        [HttpGet]
        public async Task<IActionResult> RegistrarPago(int cobroId)
        {
            try
            {
                var cobro = await _recaudadorService.ObtenerCobroPorId(cobroId);
                if (cobro == null)
                {
                    TempData["ErrorMessage"] = "El cobro especificado no existe.";
                    return RedirectToAction("Pagos");
                }

                var saldoPendiente = await _recaudadorService.ObtenerSaldoPendienteCobro(cobroId);

                ViewBag.Cobro = cobro;
                ViewBag.SaldoPendiente = saldoPendiente;

                return PartialView("_RegistrarPagoModal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar información del cobro {CobroId}", cobroId);
                TempData["ErrorMessage"] = "Error al cargar la información del cobro.";
                return RedirectToAction("Pagos");
            }
        }

        /// Procesa el registro de un pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(int cobroId, decimal valorPago, int contribuyenteId)
        {
            _logger.LogInformation("POST RegistrarPago - Cobro ID: {CobroId}, Valor: {Valor}", cobroId, valorPago);

            try
            {
                // Obtener el ID del usuario actual
                var usuarioActualId = ObtenerUsuarioActualId();
                if (usuarioActualId == 0)
                {
                    TempData["ErrorMessage"] = "No se pudo identificar al usuario actual. Por favor, inicie sesión nuevamente.";
                    return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
                }

                // Obtener el ID del recaudador basado en el usuario actual
                var recaudadorId = await _recaudadorService.ObtenerRecaudadorPorUsuario(usuarioActualId);
                if (recaudadorId == null)
                {
                    TempData["ErrorMessage"] = "El usuario actual no es un recaudador activo.";
                    return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
                }

                // Validaciones básicas
                if (valorPago <= 0)
                {
                    TempData["ErrorMessage"] = "El valor del pago debe ser mayor a cero.";
                    return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
                }

                // Registrar el pago
                var exito = await _recaudadorService.RegistrarPagoAsync(cobroId, valorPago, recaudadorId.Value);

                if (exito)
                {
                    var cobro = await _recaudadorService.ObtenerCobroPorId(cobroId);

                    // Obtener el último pago registrado para este cobro (el que acabamos de crear)
                    var pagos = await _recaudadorService.ObtenerPagosPorCobro(cobroId);
                    var ultimoPago = pagos.FirstOrDefault(); // Está ordenado por fecha desc

                    if (ultimoPago != null && cobro != null)
                    {
                        // Generar comprobante PDF
                        return await GenerarComprobantePDF(cobroId, ultimoPago.Id, contribuyenteId);
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Pago de ${valorPago:N0} registrado exitosamente para el cobro de {cobro?.per_nombre_completo}.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo registrar el pago. Intente nuevamente.";
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago");
                TempData["ErrorMessage"] = "Error interno al registrar el pago: " + ex.Message;
            }

            return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
        }

        /// Obtiene el saldo pendiente de un cobro (para AJAX)
        [HttpGet]
        public async Task<JsonResult> ObtenerSaldoPendiente(int cobroId)
        {
            try
            {
                var saldo = await _recaudadorService.ObtenerSaldoPendienteCobro(cobroId);
                return Json(new { success = true, saldo = saldo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener saldo pendiente del cobro {CobroId}", cobroId);
                return Json(new { success = false, message = "Error al obtener el saldo pendiente." });
            }
        }

        /// Obtiene los pagos realizados para un cobro (para AJAX)
        [HttpGet]
        public async Task<JsonResult> ObtenerPagosCobro(int cobroId)
        {
            try
            {
                var pagos = await _recaudadorService.ObtenerPagosPorCobro(cobroId);
                var result = pagos.Select(p => new
                {
                    p.Id,
                    p.pag_fecha,
                    p.pag_valor_pagado,
                    recaudador = p.Recaudador?.rec_activo, 
                    p.FechaRegistro
                }).ToList();

                return Json(new { success = true, pagos = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos del cobro {CobroId}", cobroId);
                return Json(new { success = false, message = "Error al obtener los pagos." });
            }
        }

        private async Task<IActionResult> GenerarComprobantePDF(int cobroId, int pagoId, int contribuyenteId)
        {
            try
            {
                // Obtener datos necesarios
                var cobro = await _recaudadorService.ObtenerCobroPorId(cobroId);
                var pago = await _recaudadorService.ObtenerPagoPorId(pagoId);

                if (cobro == null || pago == null)
                {
                    TempData["ErrorMessage"] = "Error al generar el comprobante: datos no encontrados.";
                    return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
                }

                // Obtener nombres adicionales
                var nombreRecaudador = await _recaudadorService.ObtenerNombreRecaudador(pago.rec_id);
                var nombreCompania = await _recaudadorService.ObtenerNombreCompania(cobro.com_id);

                // Generar PDF
                var pdfBytes = _comprobantePagoService.GenerarComprobantePago(
                    cobro,
                    pago,
                    nombreRecaudador,
                    nombreCompania
                );

                // Configurar nombre del archivo
                var nombreArchivo = $"Comprobante_Pago_{pago.Id:D8}_{DateTime.Now:yyyyMMdd}.pdf";

                // Guardar comprobante en la base de datos y sistema de archivos
                try
                {
                    var comprobanteId = await _recaudadorService.GuardarComprobanteAsync(
                        pagoId,
                        nombreArchivo,
                        pdfBytes,
                        "Comprobante PDF"
                    );

                    _logger.LogInformation($"Comprobante guardado con ID: {comprobanteId} para pago {pagoId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar comprobante en BD");
                    // Continuar con la descarga aunque no se haya guardado en BD
                }

                // Mensaje de éxito para mostrar después de la descarga
                TempData["SuccessMessage"] = $"Pago de ${pago.pag_valor_pagado:N0} registrado exitosamente. Comprobante generado y guardado.";

                // Retornar archivo PDF para descarga
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comprobante PDF");
                TempData["ErrorMessage"] = $"Pago registrado exitosamente, pero hubo un error al generar el comprobante: {ex.Message}";
                return RedirectToAction("Pagos", new { contribuyenteId = contribuyenteId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarComprobante(int pagoId)
        {
            try
            {
                var pago = await _recaudadorService.ObtenerPagoPorId(pagoId);
                if (pago == null)
                {
                    TempData["ErrorMessage"] = "El pago especificado no existe.";
                    return RedirectToAction("Pagos");
                }

                var cobro = await _recaudadorService.ObtenerCobroPorId(pago.cob_id);
                if (cobro == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el cobro asociado al pago.";
                    return RedirectToAction("Pagos");
                }

                var nombreRecaudador = await _recaudadorService.ObtenerNombreRecaudador(pago.rec_id);
                var nombreCompania = await _recaudadorService.ObtenerNombreCompania(cobro.com_id);

                var pdfBytes = _comprobantePagoService.GenerarComprobantePago(
                    cobro,
                    pago,
                    nombreRecaudador,
                    nombreCompania
                );

                var nombreArchivo = $"Comprobante_Pago_{pago.Id:D8}_{pago.pag_fecha:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comprobante PDF");
                TempData["ErrorMessage"] = "Error al generar el comprobante: " + ex.Message;
                return RedirectToAction("Pagos");
            }
        }


        [HttpGet]
        public async Task<JsonResult> ObtenerComprobantesPago(int pagoId)
        {
            try
            {
                _logger.LogInformation($"Buscando comprobantes para pagoId: {pagoId}");

                var comprobantes = await _recaudadorService.ObtenerComprobantesPorPago(pagoId);

                _logger.LogInformation($"Encontrados {comprobantes.Count} comprobantes para pagoId: {pagoId}");

                var result = comprobantes.Select(c => new
                {
                    c.Id,
                    c.comp_nombre_original,
                    c.comp_tipo_comprobante,
                    c.comp_tamaño_kb,
                    fechaRegistro = c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                    c.comp_descripcion,
                    pagId = c.pag_id, // Agregar para debug
                    activo = c.comp_activo // Agregar para debug
                }).ToList();

                _logger.LogInformation($"Datos a retornar: {System.Text.Json.JsonSerializer.Serialize(result)}");

                return Json(new { success = true, comprobantes = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes del pago {PagoId}", pagoId);
                return Json(new { success = false, message = "Error al obtener los comprobantes." });
            }
        }

        // Nuevo método para descargar un comprobante específico
        [HttpGet]
        public async Task<IActionResult> DescargarComprobanteEspecifico(int comprobanteId)
        {
            try
            {
                var comprobante = await _recaudadorService.ObtenerComprobantePorId(comprobanteId);
                if (comprobante == null)
                {
                    TempData["ErrorMessage"] = "El comprobante especificado no existe.";
                    return RedirectToAction("Pagos");
                }

                var contenido = await _recaudadorService.ObtenerContenidoComprobante(comprobanteId);
                if (contenido == null || contenido.Length == 0)
                {
                    TempData["ErrorMessage"] = "No se pudo obtener el contenido del comprobante.";
                    return RedirectToAction("Pagos");
                }

                var contentType = "application/pdf";
                if (!string.IsNullOrEmpty(comprobante.comp_extension))
                {
                    contentType = comprobante.comp_extension.ToLower() switch
                    {
                        ".pdf" => "application/pdf",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    };
                }

                return File(contenido, contentType, comprobante.comp_nombre_original);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar comprobante {ComprobanteId}", comprobanteId);
                TempData["ErrorMessage"] = "Error al descargar el comprobante: " + ex.Message;
                return RedirectToAction("Pagos");
            }
        }

        // Método para eliminar un comprobante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EliminarComprobante(int comprobanteId)
        {
            try
            {
                var resultado = await _recaudadorService.EliminarComprobanteAsync(comprobanteId);
                if (resultado)
                {
                    return Json(new { success = true, message = "Comprobante eliminado exitosamente." });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el comprobante." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comprobante {ComprobanteId}", comprobanteId);
                return Json(new { success = false, message = "Error al eliminar el comprobante: " + ex.Message });
            }
        }

        /// <summary>
        /// Visualiza un comprobante PDF en el navegador sin forzar descarga
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VisualizarComprobante(int pagoId)
        {
            try
            {
                var pago = await _recaudadorService.ObtenerPagoPorId(pagoId);
                if (pago == null)
                {
                    return BadRequest("El pago especificado no existe.");
                }

                var cobro = await _recaudadorService.ObtenerCobroPorId(pago.cob_id);
                if (cobro == null)
                {
                    return BadRequest("No se encontró el cobro asociado al pago.");
                }

                var nombreRecaudador = await _recaudadorService.ObtenerNombreRecaudador(pago.rec_id);
                var nombreCompania = await _recaudadorService.ObtenerNombreCompania(cobro.com_id);

                var pdfBytes = _comprobantePagoService.GenerarComprobantePago(
                    cobro,
                    pago,
                    nombreRecaudador,
                    nombreCompania
                );

                // Configurar headers para visualización en navegador (no descarga)
                Response.Headers.Add("Content-Disposition", "inline");

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al visualizar comprobante PDF para pago {PagoId}", pagoId);
                return BadRequest("Error al visualizar el comprobante.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditarPago(int pagoId, DateTime nuevaFecha)
        {
            try
            {
                var pago = await _recaudadorService.ObtenerPagoPorId(pagoId);
                if (pago == null)
                {
                    return Json(new { success = false, message = "El pago especificado no existe." });
                }

                // Validar que la fecha no sea futura
                if (nuevaFecha > DateTime.Now)
                {
                    return Json(new { success = false, message = "La fecha del pago no puede ser futura." });
                }

                var resultado = await _recaudadorService.EditarPagoAsync(pagoId, nuevaFecha);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Fecha del pago actualizada exitosamente.",
                        nuevaFecha = nuevaFecha.ToString("dd/MM/yyyy")
                    });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar la fecha del pago." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar fecha del pago {PagoId}", pagoId);
                return Json(new { success = false, message = "Error al actualizar la fecha del pago: " + ex.Message });
            }
        }

    }
}