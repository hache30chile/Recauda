using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IRolService
    {
        Task<List<Rol>> ObtenerRoles();
        Task<Rol?> ObtenerRolPorId(int id);
        Task CrearRolAsync(Rol rol);
        Task EditarRolAsync(Rol rol);
        Task EliminarRolAsync(int id);
    }
}
