using Recauda.Models;

namespace Recauda.Interfaces
{
    public interface IPersonaService
    {
        Task<List<Persona>> ObtenerPersonas();
        Task<Persona?> ObtenerPersonaPorRut(int per_rut); 
        Task CrearPersonaAsync(Persona persona);
        Task EditarPersonaAsync(Persona persona);
        Task EliminarPersonaAsync(int per_rut); 
        Task<bool> ExistePersonaConRut(int per_rut); 
    }
}