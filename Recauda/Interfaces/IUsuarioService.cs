using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerUsuarios();
        Task<Usuario?> ObtenerUsuarioPorId(int id);
        Task CrearUsuarioAsync(Usuario usuario);
        Task EditarUsuarioAsync(Usuario usuario);
        Task EliminarUsuarioAsync(int id);
        Task<List<Rol>> ObtenerRolesActivos(); // Para el dropdown de roles
    }
}
