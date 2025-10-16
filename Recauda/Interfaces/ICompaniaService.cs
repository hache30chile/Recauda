using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface ICompaniaService
    {
        Task<List<Compania>> ObtenerCompanias();
        Task<Compania?> ObtenerCompaniaPorId(int id);
        Task CrearCompaniaAsync(Compania compania);
        Task EditarCompaniaAsync(Compania compania);
        Task EliminarCompaniaAsync(int Id);
    }
}
