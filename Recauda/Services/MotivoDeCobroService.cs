using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class MotivoDeCobroService : IMotivoDeCobroService
    {
        private readonly AppDbContext _context;

        public MotivoDeCobroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MotivoDeCobro>> ObtenerMotivoDeCobros()
        {
            return await _context.MotivoDeCobros.ToListAsync();
        }

        public async Task<MotivoDeCobro?> ObtenerMotivoDeCobroPorId(int id)
        {
            return await _context.MotivoDeCobros.FindAsync(id);
        }

        public async Task CrearMotivoDeCobroAsync(MotivoDeCobro motivodecobro)
        {
            _context.MotivoDeCobros.Add(motivodecobro);
            await _context.SaveChangesAsync();
        }


        public async Task EditarMotivoDeCobroAsync(MotivoDeCobro motivodecobro)
        {
            var motivodecobroExistente = await _context.MotivoDeCobros.FindAsync(motivodecobro.Id);
            if (motivodecobroExistente != null)
            {
                motivodecobroExistente.mdc_nombre = motivodecobro.mdc_nombre;
                motivodecobroExistente.mdc_activo = motivodecobro.mdc_activo;
                // Actualiza otras propiedades si existen
                await _context.SaveChangesAsync();
            }
        }



        public async Task EliminarMotivoDeCobroAsync(int id)
        {
            var motivodecobro = await _context.MotivoDeCobros.FindAsync(id);
            if (motivodecobro != null)
            {
                _context.MotivoDeCobros.Remove(motivodecobro);
                await _context.SaveChangesAsync();
            }
        }
    }
}