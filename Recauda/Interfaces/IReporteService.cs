using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IReporteService
    {
        // Métodos para reporte de ingresos
        Task<List<Compania>> ObtenerCompaniasActivas();
        Task<List<ReporteIngreso>> ObtenerReporteIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin);
        Task<ResumenIngresos> ObtenerResumenIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin);
    }

    // Clase para representar un ingreso en el reporte
    public class ReporteIngreso
    {
        public int PagoId { get; set; }
        public DateTime FechaPago { get; set; }
        public string NombreContribuyente { get; set; }
        public int RutContribuyente { get; set; }
        public string RutFormateado { get; set; }
        public string NombreCompania { get; set; }
        public string MotivoCobro { get; set; }
        public string PeriodoCobro { get; set; }
        public decimal ValorPagado { get; set; }
        public string NombreRecaudador { get; set; }
    }

    // Clase para el resumen de ingresos
    public class ResumenIngresos
    {
        public int TotalPagos { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PromedioIngreso { get; set; }
        public decimal IngresoMayor { get; set; }
        public decimal IngresoMenor { get; set; }
        public Dictionary<string, decimal> IngresosPorCompania { get; set; }
        public Dictionary<string, decimal> IngresosPorMes { get; set; }
    }
}