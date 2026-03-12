using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Recauda.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Security.Claims;

namespace Recauda.Controllers
{
    [Authorize]
    public class ReporteController : Controller
    {
        private readonly IReporteService _reporteService;
        private readonly ILogger<ReporteController> _logger;

        public ReporteController(IReporteService reporteService, ILogger<ReporteController> logger)
        {
            _reporteService = reporteService;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReporteIngresos()
        {
            try
            {
                await CargarCompanias();
                await CargarCompaniaFija();
                ViewBag.FechaInicio = null;
                ViewBag.FechaFin = null;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de reporte de ingresos");
                TempData["ErrorMessage"] = "Error al cargar el reporte: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            _logger.LogInformation($"POST ReporteIngresos - Compañía: {companiaId}, Inicio: {fechaInicio}, Fin: {fechaFin}");

            try
            {
                // Si es Tesorero, ignorar el companiaId del form y forzar el suyo
                companiaId = await ObtenerCompaniaEfectiva(companiaId);

                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    ModelState.AddModelError("", "La fecha de inicio no puede ser mayor a la fecha de fin.");
                    await CargarCompanias();
                    await CargarCompaniaFija();
                    ViewBag.FechaInicio = fechaInicio;
                    ViewBag.FechaFin = fechaFin;
                    ViewBag.CompaniaSeleccionada = companiaId;
                    return View("ReporteIngresos");
                }

                var ingresos = await _reporteService.ObtenerReporteIngresos(companiaId, fechaInicio, fechaFin);
                var resumen = await _reporteService.ObtenerResumenIngresos(companiaId, fechaInicio, fechaFin);

                ViewBag.Ingresos = ingresos;
                ViewBag.Resumen = resumen;
                ViewBag.FechaInicio = fechaInicio;
                ViewBag.FechaFin = fechaFin;
                ViewBag.CompaniaSeleccionada = companiaId;

                TempData[ingresos.Count == 0 ? "InfoMessage" : "SuccessMessage"] = ingresos.Count == 0
                    ? "No se encontraron ingresos con los filtros aplicados."
                    : $"Se encontraron {ingresos.Count} registro(s) de ingresos.";

                await CargarCompanias();
                await CargarCompaniaFija();
                return View("ReporteIngresos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ingresos");
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                await CargarCompanias();
                await CargarCompaniaFija();
                return View("ReporteIngresos");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarReporteExcel(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Si es Tesorero, forzar su compañía también en la exportación
                companiaId = await ObtenerCompaniaEfectiva(companiaId);

                var ingresos = await _reporteService.ObtenerReporteIngresos(companiaId, fechaInicio, fechaFin);

                if (ingresos.Count == 0)
                {
                    TempData["ErrorMessage"] = "No hay datos para exportar.";
                    return RedirectToAction("ReporteIngresos");
                }

                var excelBytes = GenerarExcel(ingresos);
                var nombreArchivo = $"Reporte_Ingresos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte a Excel");
                TempData["ErrorMessage"] = "Error al exportar el reporte: " + ex.Message;
                return RedirectToAction("ReporteIngresos");
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private async Task CargarCompanias()
        {
            try
            {
                var companias = await _reporteService.ObtenerCompaniasActivas();
                ViewBag.Companias = new SelectList(companias, "Id", "com_nombre");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compañías");
                ViewBag.Companias = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        /// <summary>
        /// Si el usuario es Tesorero, setea ViewBag.CompaniaIdFija y ViewBag.CompaniaNombreFija
        /// para que la vista bloquee el selector de compañía.
        /// </summary>
        private async Task CargarCompaniaFija()
        {
            var rolNombre = User.FindFirstValue(ClaimTypes.Role);
            if (rolNombre?.ToLower() != "tesorero") return;

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companiaId = await _reporteService.ObtenerCompaniaDeUsuario(usuarioId);

            if (companiaId.HasValue)
            {
                var companias = await _reporteService.ObtenerCompaniasActivas();
                var nombre = companias.FirstOrDefault(c => c.Id == companiaId.Value)?.com_nombre ?? string.Empty;

                ViewBag.CompaniaIdFija = companiaId.Value;
                ViewBag.CompaniaNombreFija = nombre;
            }
        }

        /// <summary>
        /// Retorna el companiaId efectivo: si el usuario es Tesorero devuelve siempre su compañía,
        /// ignorando el valor que llegue del formulario.
        /// </summary>
        private async Task<int?> ObtenerCompaniaEfectiva(int? companiaIdForm)
        {
            var rolNombre = User.FindFirstValue(ClaimTypes.Role);
            if (rolNombre?.ToLower() != "tesorero") return companiaIdForm;

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _reporteService.ObtenerCompaniaDeUsuario(usuarioId) ?? companiaIdForm;
        }

        private byte[] GenerarExcel(List<ReporteIngreso> ingresos)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reporte de Ingresos");

            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = "REPORTE DE INGRESOS";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Row(1).Height = 25;

            worksheet.Cells["A2"].Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Cells["A2"].Style.Font.Italic = true;
            worksheet.Row(2).Height = 20;

            var headers = new[] { "Fecha Pago", "Contribuyente", "RUT", "Compañía", "Motivo", "Período", "Valor Pagado", "Recaudador" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[4, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            worksheet.Row(4).Height = 25;

            int row = 5;
            decimal total = 0;
            foreach (var ingreso in ingresos)
            {
                worksheet.Cells[row, 1].Value = ingreso.FechaPago.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 2].Value = ingreso.NombreContribuyente;
                worksheet.Cells[row, 3].Value = ingreso.RutFormateado;
                worksheet.Cells[row, 4].Value = ingreso.NombreCompania;
                worksheet.Cells[row, 5].Value = ingreso.MotivoCobro;
                worksheet.Cells[row, 6].Value = ingreso.PeriodoCobro;
                worksheet.Cells[row, 7].Value = ingreso.ValorPagado;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0";
                worksheet.Cells[row, 8].Value = ingreso.NombreRecaudador;

                for (int col = 1; col <= 8; col++)
                    worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                if (row % 2 == 0)
                {
                    worksheet.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                }

                total += ingreso.ValorPagado;
                row++;
            }

            worksheet.Cells[row, 1, row, 6].Merge = true;
            worksheet.Cells[row, 1].Value = "TOTAL:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
            worksheet.Cells[row, 7].Value = total;
            worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0";
            worksheet.Cells[row, 7].Style.Font.Bold = true;
            worksheet.Cells[row, 7].Style.Font.Size = 12;
            worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(146, 208, 80));
            for (int col = 1; col <= 8; col++)
                worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            worksheet.Row(row).Height = 25;

            worksheet.Column(1).Width = 18;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 25;
            worksheet.Column(5).Width = 25;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 25;
            worksheet.View.FreezePanes(5, 1);

            return package.GetAsByteArray();
        }
    }
}