using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly AppDbContext _context;

        public PersonaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Persona>> ObtenerPersonas()
        {
            return await _context.Personas
                .OrderBy(p => p.per_rut) 
                .ToListAsync();
        }

        public async Task<Persona?> ObtenerPersonaPorRut(int per_rut)
        {
            return await _context.Personas
                .FirstOrDefaultAsync(p => p.per_rut == per_rut);
        }

        public async Task CrearPersonaAsync(Persona persona)
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

        public async Task EditarPersonaAsync(Persona persona)
        {
            var personaExistente = await _context.Personas
                .FirstOrDefaultAsync(p => p.per_rut == persona.per_rut);

            if (personaExistente != null)
            {
                personaExistente.per_vrut = persona.per_vrut;
                personaExistente.per_paterno = persona.per_paterno;
                personaExistente.per_materno = persona.per_materno;
                personaExistente.per_nombres = persona.per_nombres;
                personaExistente.sex_codigo = persona.sex_codigo;
                personaExistente.per_fecnac = persona.per_fecnac;
                personaExistente.per_email = persona.per_email;
                personaExistente.per_movil = persona.per_movil;
                personaExistente.per_calle = persona.per_calle;
                personaExistente.per_numero = persona.per_numero;
                personaExistente.per_depto = persona.per_depto;
                personaExistente.per_block = persona.per_block;
                personaExistente.per_comuna = persona.per_comuna;

                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarPersonaAsync(int per_rut)
        {
            var persona = await _context.Personas
                .FirstOrDefaultAsync(p => p.per_rut == per_rut);

            if (persona != null)
            {
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
            }
        }

        // AGREGADO: Método útil para verificar existencia
        public async Task<bool> ExistePersonaConRut(int per_rut)
        {
            return await _context.Personas
                .AnyAsync(p => p.per_rut == per_rut);
        }
    }
}