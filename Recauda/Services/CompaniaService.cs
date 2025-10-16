using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class CompaniaService : ICompaniaService
    {
        private readonly AppDbContext _context;

        public CompaniaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Compania>> ObtenerCompanias()
        {
            return await _context.Companias.ToListAsync();
        }

        public async Task<Compania?> ObtenerCompaniaPorId(int id)
        {
            return await _context.Companias.FindAsync(id);
        }

        public async Task CrearCompaniaAsync(Compania compania)
        {
            _context.Companias.Add(compania);
            await _context.SaveChangesAsync();
        }
  

        public async Task EditarCompaniaAsync(Compania compania)
        {
            var companiaExistente = await _context.Companias.FindAsync(compania.Id);
            if (companiaExistente != null)
            {
                companiaExistente.com_nombre = compania.com_nombre;
                // Actualiza otras propiedades si existen
                await _context.SaveChangesAsync();
            }
        }



        public async Task EliminarCompaniaAsync(int id)
        {
            var compania = await _context.Companias.FindAsync(id);
            if (compania != null)
            {
                _context.Companias.Remove(compania);
                await _context.SaveChangesAsync();
            }
        }
    }
}