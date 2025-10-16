using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerUsuarios();
        Task<Usuario?> ObtenerUsuarioPorId(int id);
        Task CrearUsuarioAsync(Usuario usuario, bool esGenerador = false);
        Task EditarUsuarioAsync(Usuario usuario, bool esGenerador = false);
        Task EliminarUsuarioAsync(int id);
        Task<List<Rol>> ObtenerRolesActivos(); 
        Task<bool> EsUsuarioGenerador(int usuarioId); 
    }
}