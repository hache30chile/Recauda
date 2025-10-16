using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Recauda.Models;

namespace Recauda.Services
{
    public class ContribuyenteService : IContribuyenteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public ContribuyenteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VContribuyente>> ObtenerContribuyentes()
        {
            return await _context.VContribuyentes
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<VContribuyente?> ObtenerContribuyentePorId(int Id)
        {
            return await _context.VContribuyentes
                .FirstOrDefaultAsync(p => p.Id == Id);
        }

        public async Task CrearContribuyenteAsync(Contribuyente contribuyente)
        {
            _context.Contribuyentes.Add(contribuyente);
            await _context.SaveChangesAsync();
        }

        public async Task EditarContribuyenteAsync(Contribuyente contribuyente)
        {
            var contribuyenteExistente = await _context.Contribuyentes
                .FirstOrDefaultAsync(p => p.Id == contribuyente.Id);

            if (contribuyenteExistente != null)
            {
                contribuyenteExistente.per_rut = contribuyente.per_rut;
                contribuyenteExistente.mdc_id = contribuyente.mdc_id;
                contribuyenteExistente.con_valor_aporte = contribuyente.con_valor_aporte;
                contribuyenteExistente.con_periodicidad_cobro = contribuyente.con_periodicidad_cobro;
                contribuyenteExistente.con_dia_del_cargo = contribuyente.con_dia_del_cargo;
                contribuyenteExistente.con_fecha_inicio = contribuyente.con_fecha_inicio;
                contribuyenteExistente.con_fecha_fin = contribuyente.con_fecha_fin;
                contribuyenteExistente.rec_id = contribuyente.rec_id;
                contribuyenteExistente.con_activo = contribuyente.con_activo;
                contribuyenteExistente.com_id = contribuyente.com_id;                
                contribuyenteExistente.FechaRegistro = contribuyente.FechaRegistro;
                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarContribuyenteAsync(int Id)
        {
            var contribuyente = await _context.Contribuyentes
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (contribuyente != null)
            {
                _context.Contribuyentes.Remove(contribuyente);
                await _context.SaveChangesAsync();
            }
        }

        // AGREGADO: Método útil para verificar existencia
        public async Task<bool> ExisteContribuyenteConId(int Id)
        {
            return await _context.Contribuyentes
                .AnyAsync(p => p.Id == Id);
        }










        // Buscar persona por RUT
        public async Task<Persona?> BuscarPersonaPorRut(int rut)
        {
            return await _context.Personas
                .FirstOrDefaultAsync(p => p.per_rut == rut);
        }

        // Crear nueva persona
        public async Task CrearPersonaAsync(Persona persona)
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

        // Obtener lista de motivos de cobro para dropdown
        public async Task<List<SelectListItem>> ObtenerMotivosCobro()
        {
            return await _context.MotivoDeCobros
                .Where(m => m.mdc_activo)
                .OrderBy(m => m.mdc_nombre)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.mdc_nombre
                })
                .ToListAsync();
        }

        // Obtener lista de recaudadores para dropdown
        public async Task<List<SelectListItem>> ObtenerRecaudadores()
        {
            return await _context.VRecaudadores
                .Where(r => r.rec_activo)
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.Nombre} - {r.com_nombre}"
                })
                .ToListAsync();
        }

        // Obtener lista de compañías para dropdown
        public async Task<List<SelectListItem>> ObtenerCompanias()
        {
            return await _context.Companias
                .OrderBy(c => c.com_nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.com_nombre
                })
                .ToListAsync();
        }

    }
}