using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IMotivoDeCobroService
    {
        Task<List<MotivoDeCobro>> ObtenerMotivoDeCobros();
        Task<MotivoDeCobro?> ObtenerMotivoDeCobroPorId(int id);
        Task CrearMotivoDeCobroAsync(MotivoDeCobro motivodecobro);
        Task EditarMotivoDeCobroAsync(MotivoDeCobro motivodecobro);
        Task EliminarMotivoDeCobroAsync(int Id);
    }
}
