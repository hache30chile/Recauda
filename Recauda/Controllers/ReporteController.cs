using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Recauda.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

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

            // Configurar licencia de EPPlus (Non-Commercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Vista del reporte de ingresos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ReporteIngresos()
        {
            try
            {
                await CargarCompanias();

                // No configurar fechas por defecto, dejar vacío para traer todo
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

        /// <summary>
        /// Genera el reporte de ingresos con los filtros aplicados
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            _logger.LogInformation("POST ReporteIngresos - Generando reporte");
            _logger.LogInformation($"Filtros - Compañía: {companiaId}, Fecha Inicio: {fechaInicio}, Fecha Fin: {fechaFin}");

            try
            {
                // Validar fechas solo si ambas están presentes
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    ModelState.AddModelError("", "La fecha de inicio no puede ser mayor a la fecha de fin.");
                    await CargarCompanias();
                    ViewBag.FechaInicio = fechaInicio;
                    ViewBag.FechaFin = fechaFin;
                    ViewBag.CompaniaSeleccionada = companiaId;
                    return View("ReporteIngresos");
                }

                // Obtener datos del reporte (sin filtros de fecha trae todo el histórico)
                var ingresos = await _reporteService.ObtenerReporteIngresos(companiaId, fechaInicio, fechaFin);
                var resumen = await _reporteService.ObtenerResumenIngresos(companiaId, fechaInicio, fechaFin);

                // Pasar datos a la vista
                ViewBag.Ingresos = ingresos;
                ViewBag.Resumen = resumen;
                ViewBag.FechaInicio = fechaInicio;
                ViewBag.FechaFin = fechaFin;
                ViewBag.CompaniaSeleccionada = companiaId;

                if (ingresos.Count == 0)
                {
                    TempData["InfoMessage"] = "No se encontraron ingresos con los filtros aplicados.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Se encontraron {ingresos.Count} registro(s) de ingresos.";
                }

                await CargarCompanias();
                return View("ReporteIngresos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ingresos");
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                await CargarCompanias();
                return View("ReporteIngresos");
            }
        }

        /// <summary>
        /// Carga la lista de compañías activas para el dropdown
        /// </summary>
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
        /// Exporta el reporte a Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarReporteExcel(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
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

        /// <summary>
        /// Genera el archivo Excel del reporte
        /// </summary>
        private byte[] GenerarExcel(List<ReporteIngreso> ingresos)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reporte de Ingresos");

            // Título del reporte
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = "REPORTE DE INGRESOS";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
            worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            worksheet.Row(1).Height = 25;

            // Información adicional
            worksheet.Cells["A2"].Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Cells["A2"].Style.Font.Italic = true;
            worksheet.Row(2).Height = 20;

            // Encabezados de columnas
            var headers = new string[]
            {
                "Fecha Pago",
                "Contribuyente",
                "RUT",
                "Compañía",
                "Motivo",
                "Período",
                "Valor Pagado",
                "Recaudador"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[4, i + 1].Value = headers[i];
                worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                worksheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                worksheet.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            worksheet.Row(4).Height = 25;

            // Datos
            int row = 5;
            decimal totalIngresos = 0;

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

                // Bordes para todas las celdas
                for (int col = 1; col <= 8; col++)
                {
                    worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Alternar color de filas para mejor legibilidad
                if (row % 2 == 0)
                {
                    worksheet.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                }

                totalIngresos += ingreso.ValorPagado;
                row++;
            }

            // Fila de totales
            worksheet.Cells[row, 1, row, 6].Merge = true;
            worksheet.Cells[row, 1].Value = "TOTAL:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));

            worksheet.Cells[row, 7].Value = totalIngresos;
            worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0";
            worksheet.Cells[row, 7].Style.Font.Bold = true;
            worksheet.Cells[row, 7].Style.Font.Size = 12;
            worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(146, 208, 80));

            // Bordes para la fila de totales
            for (int col = 1; col <= 8; col++)
            {
                worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            }

            worksheet.Row(row).Height = 25;

            // Ajustar ancho de columnas
            worksheet.Column(1).Width = 18; // Fecha
            worksheet.Column(2).Width = 30; // Contribuyente
            worksheet.Column(3).Width = 15; // RUT
            worksheet.Column(4).Width = 25; // Compañía
            worksheet.Column(5).Width = 25; // Motivo
            worksheet.Column(6).Width = 12; // Período
            worksheet.Column(7).Width = 15; // Valor
            worksheet.Column(8).Width = 25; // Recaudador

            // Congelar paneles (encabezados)
            worksheet.View.FreezePanes(5, 1);

            return package.GetAsByteArray();
        }
    }
}