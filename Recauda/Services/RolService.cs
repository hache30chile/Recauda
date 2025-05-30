using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class RolService : IRolService
    {
        private readonly AppDbContext _context;

        public RolService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rol>> ObtenerRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Rol?> ObtenerRolPorId(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task CrearRolAsync(Rol rol)
        {
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
        }

        public async Task EditarRolAsync(Rol rol)
        {
            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarRolAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol != null)
            {
                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();
            }
        }
    }
}
