using Recauda.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Recauda.Interfaces
{
    public interface IContribuyenteService
    {
        //Task<List<Contribuyente>> ObtenerContribuyentes();
        Task<List<VContribuyente>> ObtenerContribuyentes();
        Task<VContribuyente?> ObtenerContribuyentePorId(int Id); 
        Task CrearContribuyenteAsync(Contribuyente contribuyente);
        Task EditarContribuyenteAsync(Contribuyente contribuyente);
        Task EliminarContribuyenteAsync(int Id); 
        Task<bool> ExisteContribuyenteConId(int Id);

        // Métodos para manejo de personas
        Task<Persona?> BuscarPersonaPorRut(int rut);
        Task CrearPersonaAsync(Persona persona);

        // Métodos para obtener datos de dropdowns
        Task<List<SelectListItem>> ObtenerMotivosCobro();
        Task<List<SelectListItem>> ObtenerRecaudadores();
        Task<List<SelectListItem>> ObtenerCompanias();

    }
}