using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class ReporteService : IReporteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(AppDbContext context, ILogger<ReporteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Compania>> ObtenerCompaniasActivas()
        {
            return await _context.Companias
                .OrderBy(c => c.com_nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna el com_id del generador activo del usuario (usado para filtrar por compañía del Tesorero).
        /// </summary>
        public async Task<int?> ObtenerCompaniaDeUsuario(int usuarioId)
        {
            var generador = await _context.Generadores
                .FirstOrDefaultAsync(g => g.usu_id == usuarioId && g.gen_activo);
            return generador?.com_id;
        }

        public async Task<List<ReporteIngreso>> ObtenerReporteIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = from pago in _context.Pagos
                            join cobro in _context.Cobros on pago.cob_id equals cobro.Id
                            join contribuyente in _context.Contribuyentes on cobro.con_id equals contribuyente.Id
                            join persona in _context.Personas on contribuyente.per_rut equals persona.per_rut
                            join compania in _context.Companias on cobro.com_id equals compania.Id
                            join motivo in _context.MotivoDeCobros on cobro.mdc_id equals motivo.Id
                            join recaudador in _context.Recaudadores on pago.rec_id equals recaudador.Id
                            join usuarioRec in _context.Usuarios on recaudador.usu_id equals usuarioRec.Id
                            select new
                            {
                                Pago = pago,
                                Cobro = cobro,
                                Persona = persona,
                                Compania = compania,
                                Motivo = motivo,
                                UsuarioRecaudador = usuarioRec
                            };

                if (companiaId.HasValue && companiaId.Value > 0)
                    query = query.Where(x => x.Cobro.com_id == companiaId.Value);

                if (fechaInicio.HasValue)
                    query = query.Where(x => x.Pago.pag_fecha >= fechaInicio.Value);

                if (fechaFin.HasValue)
                {
                    var fechaFinInclusive = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.Pago.pag_fecha <= fechaFinInclusive);
                }

                var resultados = await query
                    .OrderByDescending(x => x.Pago.pag_fecha)
                    .ToListAsync();

                return resultados.Select(x => new ReporteIngreso
                {
                    PagoId = x.Pago.Id,
                    FechaPago = x.Pago.pag_fecha,
                    NombreContribuyente = x.Persona.per_nombres,
                    RutContribuyente = x.Persona.per_rut,
                    RutFormateado = FormatearRut(x.Persona.per_rut, x.Persona.per_vrut),
                    NombreCompania = x.Compania.com_nombre,
                    MotivoCobro = x.Motivo.mdc_nombre,
                    PeriodoCobro = x.Cobro.cob_fecha_emision.ToString("MM/yyyy"),
                    ValorPagado = x.Pago.pag_valor_pagado,
                    NombreRecaudador = x.UsuarioRecaudador.Nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ingresos");
                throw;
            }
        }

        public async Task<ResumenIngresos> ObtenerResumenIngresos(int? companiaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var ingresos = await ObtenerReporteIngresos(companiaId, fechaInicio, fechaFin);

                if (!ingresos.Any())
                {
                    return new ResumenIngresos
                    {
                        TotalPagos = 0,
                        TotalIngresos = 0,
                        PromedioIngreso = 0,
                        IngresoMayor = 0,
                        IngresoMenor = 0,
                        IngresosPorCompania = new Dictionary<string, decimal>(),
                        IngresosPorMes = new Dictionary<string, decimal>()
                    };
                }

                return new ResumenIngresos
                {
                    TotalPagos = ingresos.Count,
                    TotalIngresos = ingresos.Sum(x => x.ValorPagado),
                    PromedioIngreso = ingresos.Average(x => x.ValorPagado),
                    IngresoMayor = ingresos.Max(x => x.ValorPagado),
                    IngresoMenor = ingresos.Min(x => x.ValorPagado),
                    IngresosPorCompania = ingresos
                        .GroupBy(x => x.NombreCompania)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.ValorPagado)),
                    IngresosPorMes = ingresos
                        .GroupBy(x => x.FechaPago.ToString("yyyy-MM"))
                        .OrderBy(g => g.Key)
                        .ToDictionary(
                            g => FormatearMesAnio(g.Key),
                            g => g.Sum(x => x.ValorPagado)
                        )
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de ingresos");
                throw;
            }
        }

        private string FormatearRut(int rut, string dv)
            => $"{rut:N0}".Replace(",", ".") + "-" + dv;

        private string FormatearMesAnio(string yyyyMM)
        {
            if (DateTime.TryParseExact(yyyyMM + "-01", "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out DateTime fecha))
            {
                return fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-CL"));
            }
            return yyyyMM;
        }
    }
}