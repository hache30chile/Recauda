using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IRecaudadorService
    {
        // Métodos básicos de recaudadores
        Task<List<VRecaudador>> ObtenerRecaudadores();
        Task<VRecaudador?> ObtenerRecaudadorPorId(int Id);
        Task CrearRecaudadorAsync(Recaudador recaudador);
        Task EditarRecaudadorAsync(Recaudador recaudador);
        Task EliminarRecaudadorAsync(int Id);
        Task<bool> ExisteRecaudadorConId(int Id);

        // Métodos para obtener datos relacionados
        Task<List<Usuario>> ObtenerUsuariosActivos();
        Task<List<Compania>> ObtenerCompaniasActivos();

        // Métodos para generación de cobros
        Task<List<VContribuyente>> ObtenerContribuyentesActivos();
        Task<VContribuyente?> ObtenerContribuyentePorId(int id);
        Task<int> GenerarCobrosAsync(VContribuyente contribuyente, int anio, int usuarioActualId);
        Task<int?> ObtenerGeneradorPorUsuario(int usuarioId);

        // Métodos para consulta de cobros
        Task<List<VCobros>> ObtenerTodosLosCobros();
        Task<List<VCobros>> BuscarCobrosPorRut(int rut);

        // Métodos para manejo de pagos
        Task<List<VCobros>> ObtenerCobrosPorContribuyente(int contribuyenteId);
        Task<VCobros?> ObtenerCobroPorId(int cobroId);
        Task<bool> RegistrarPagoAsync(int cobroId, decimal valorPagado, int recaudadorId);
        Task<decimal> ObtenerSaldoPendienteCobro(int cobroId);
        Task<List<Pagos>> ObtenerPagosPorCobro(int cobroId);
        Task<int?> ObtenerRecaudadorPorUsuario(int usuarioId);

        // Métodos para obtener información adicional
        Task<string> ObtenerNombreRecaudador(int recaudadorId);
        Task<string> ObtenerNombreCompania(int companiaId);
        Task<Pagos?> ObtenerPagoPorId(int pagoId);

        // Métodos para manejo de comprobantes
        Task<int> GuardarComprobanteAsync(int pagoId, string nombreArchivo, byte[] contenidoArchivo, string tipoComprobante = "PDF");
        Task<List<ComprobantesPago>> ObtenerComprobantesPorPago(int pagoId);
        Task<ComprobantesPago?> ObtenerComprobantePorId(int comprobanteId);
        Task<bool> EliminarComprobanteAsync(int comprobanteId);
        Task<byte[]?> ObtenerContenidoComprobante(int comprobanteId);
        // Agregar este método a la interfaz
        Task<bool> EditarPagoAsync(int pagoId, DateTime nuevaFecha);
    }
}