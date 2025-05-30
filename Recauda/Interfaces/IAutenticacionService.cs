using Recauda.Models;
using Recauda.Models.DTOs;

namespace Recauda.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario?> ValidarCredencialesAsync(string login, string clave);
        Task<bool> CambiarClaveAsync(int usuarioId, string claveActual, string claveNueva);
        string HashearClave(string clave);
        bool VerificarClave(string clave, string hashAlmacenado);
    }
}