using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerUsuarios();
        Task<Usuario?> ObtenerUsuarioPorId(int id);
        Task CrearUsuarioAsync(Usuario usuario, bool esGenerador = false, int? companiaId = null);
        Task EditarUsuarioAsync(Usuario usuario, bool esGenerador = false, int? companiaId = null);
        Task EliminarUsuarioAsync(int id);
        Task<List<Rol>> ObtenerRolesActivos();
        Task<List<Compania>> ObtenerCompanias();
        Task<bool> EsUsuarioGenerador(int usuarioId);
        Task<int?> ObtenerIdRolTesorero();
        Task<bool> CompaniaTieneTesorero(int companiaId, int? excludeUsuarioId = null);
        Task<int?> ObtenerCompaniaDeTesorero(int usuarioId);
    }
}