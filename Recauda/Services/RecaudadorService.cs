using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using Recauda.Interfaces;
using Recauda.Models;

namespace Recauda.Services
{
    public class RecaudadorService : IRecaudadorService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public RecaudadorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VRecaudador>> ObtenerRecaudadores()
        {
            return await _context.VRecaudadores
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<VRecaudador?> ObtenerRecaudadorPorId(int Id)
        {
            return await _context.VRecaudadores
                .FirstOrDefaultAsync(p => p.Id == Id);
        }

        public async Task CrearRecaudadorAsync(Recaudador recaudador)
        {
            _context.Recaudadores.Add(recaudador);
            await _context.SaveChangesAsync();
        }

        public async Task EditarRecaudadorAsync(Recaudador recaudador)
        {
            var recaudadorExistente = await _context.Recaudadores
                .FirstOrDefaultAsync(p => p.Id == recaudador.Id);

            if (recaudadorExistente != null)
            {
                recaudadorExistente.rec_activo = recaudador.rec_activo;
                recaudadorExistente.usu_id = recaudador.usu_id;
                recaudadorExistente.com_id = recaudador.com_id;
                recaudadorExistente.FechaRegistro = recaudador.FechaRegistro;
                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarRecaudadorAsync(int Id)
        {
            var recaudador = await _context.Recaudadores
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (recaudador != null)
            {
                _context.Recaudadores.Remove(recaudador);
                await _context.SaveChangesAsync();
            }
        }

        // Método  para verificar existencia
        public async Task<bool> ExisteRecaudadorConId(int Id)
        {
            return await _context.Recaudadores
                .AnyAsync(p => p.Id == Id);
        }

        public async Task<List<Usuario>> ObtenerUsuariosActivos()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<List<Compania>> ObtenerCompaniasActivos()
        {
            return await _context.Companias.ToListAsync();
        }

        /// Obtiene el ID del generador basado en el usuario actual
        public async Task<int?> ObtenerGeneradorPorUsuario(int usuarioId)
        {
            var generador = await _context.Generadores
                .Where(g => g.usu_id == usuarioId && g.gen_activo)
                .FirstOrDefaultAsync();

            return generador?.Id;
        }

        /// Obtiene todos los contribuyentes activos con periodicidad mensual
        public async Task<List<VContribuyente>> ObtenerContribuyentesActivos()
        {
            return await _context.VContribuyentes
                .Where(c => c.con_activo && c.con_periodicidad_cobro.ToLower() == "mensual")
                .OrderBy(c => c.per_nombre_completo)
                .ToListAsync();
        }

        /// Obtiene un contribuyente por su ID
        public async Task<VContribuyente?> ObtenerContribuyentePorId(int id)
        {
            return await _context.VContribuyentes
                .FirstOrDefaultAsync(c => c.Id == id);
        }


        /// Genera cobros MENSUALES para un contribuyente en un año específico
        public async Task<int> GenerarCobrosAsync(VContribuyente contribuyente, int anio, int usuarioActualId)
        {
            // Validar que la periodicidad sea mensual
            if (contribuyente.con_periodicidad_cobro.ToLower() != "mensual")
            {
                throw new InvalidOperationException($"Solo se permite periodicidad mensual. El contribuyente tiene: {contribuyente.con_periodicidad_cobro}");
            }

            // Obtener el gen_id basado en el usuario actual
            var generadorId = await ObtenerGeneradorPorUsuario(usuarioActualId);

            if (generadorId == null)
            {
                throw new InvalidOperationException($"El usuario con ID {usuarioActualId} no es un generador activo o no existe en la tabla Generadores.");
            }

            var cobrosGenerados = 0;
            var fechasGeneracion = CalcularFechasMensuales(contribuyente, anio);

            foreach (var fechaGeneracion in fechasGeneracion)
            {
                // Verificar si ya existe un cobro para esta fecha y contribuyente
                var cobroExistente = await _context.Cobros
                    .AnyAsync(c => c.con_id == contribuyente.Id
                              && c.cob_fecha_emision.Year == fechaGeneracion.Year
                              && c.cob_fecha_emision.Month == fechaGeneracion.Month);

                if (!cobroExistente)
                {
                    var nuevoCobro = new Cobros
                    {
                        cob_fecha_emision = fechaGeneracion,
                        cob_fecha_vencimiento = CalcularFechaVencimiento(fechaGeneracion, contribuyente.con_dia_del_cargo),
                        mdc_id = contribuyente.mdc_id,
                        cob_valor = contribuyente.con_valor_aporte,
                        con_id = contribuyente.Id,
                        gen_id = generadorId.Value,
                        com_id = contribuyente.com_id,
                        FechaRegistro = DateTime.Now
                    };

                    _context.Cobros.Add(nuevoCobro);
                    cobrosGenerados++;
                }
            }

            if (cobrosGenerados > 0)
            {
                await _context.SaveChangesAsync();
            }

            return cobrosGenerados;
        }

        private List<DateTime> CalcularFechasMensuales(VContribuyente contribuyente, int anio)
        {
            var fechas = new List<DateTime>();
            var fechaInicio = contribuyente.con_fecha_inicio;
            var fechaFin = contribuyente.con_fecha_fin ?? new DateTime(anio, 12, 31);

            // Asegurar que trabajamos dentro del año solicitado
            var inicioAnio = new DateTime(anio, 1, 1);
            var finAnio = new DateTime(anio, 12, 31);

            if (fechaInicio > finAnio || (fechaFin < inicioAnio))
            {
                return fechas; // No hay fechas válidas en este año
            }

            // Ajustar las fechas al año solicitado
            var fechaInicioCalculo = fechaInicio > inicioAnio ? fechaInicio : inicioAnio;
            var fechaFinCalculo = fechaFin < finAnio ? fechaFin : finAnio;

            // Generar fechas mensuales desde la fecha de inicio de vigencia
            var fechaActual = new DateTime(fechaInicioCalculo.Year, fechaInicioCalculo.Month,
                Math.Min(contribuyente.con_dia_del_cargo, DateTime.DaysInMonth(fechaInicioCalculo.Year, fechaInicioCalculo.Month)));

            while (fechaActual <= fechaFinCalculo)
            {
                // Solo agregar si la fecha actual es mayor o igual a la fecha de inicio del contribuyente
                if (fechaActual >= fechaInicioCalculo)
                {
                    fechas.Add(fechaActual);
                }

                // Avanzar al siguiente mes
                fechaActual = fechaActual.AddMonths(1);
                fechaActual = new DateTime(fechaActual.Year, fechaActual.Month,
                    Math.Min(contribuyente.con_dia_del_cargo, DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month)));
            }

            return fechas;
        }

        /// Calcula la fecha de vencimiento basada en la fecha de emisión y el día de cargo
        private DateTime CalcularFechaVencimiento(DateTime fechaEmision, int diaCargo)
        {
            // La fecha de vencimiento es el mismo día del cargo del mes siguiente
            var fechaVencimiento = fechaEmision.AddMonths(1);
            var diasEnMes = DateTime.DaysInMonth(fechaVencimiento.Year, fechaVencimiento.Month);
            var diaVencimiento = Math.Min(diaCargo, diasEnMes);

            return new DateTime(fechaVencimiento.Year, fechaVencimiento.Month, diaVencimiento);
        }

        /// Obtiene todos los cobros con información del contribuyente y persona
        public async Task<List<VCobros>> ObtenerTodosLosCobros()
        {
            return await _context.VCobros
                .OrderByDescending(c => c.cob_fecha_emision)
                .ToListAsync();
        }

        /// Busca cobros por RUT del contribuyente
        public async Task<List<VCobros>> BuscarCobrosPorRut(int rut)
        {
            return await _context.VCobros
                .Where(c => c.per_rut == rut)
                .OrderByDescending(c => c.cob_fecha_emision)
                .ToListAsync();
        }


        /// Formatea RUT para búsqueda (elimina puntos y guión)
        public static int? FormatearRutParaBusqueda(string rutTexto)
        {
            if (string.IsNullOrWhiteSpace(rutTexto))
                return null;

            // Eliminar puntos, guiones y espacios
            var rutLimpio = rutTexto.Replace(".", "").Replace("-", "").Replace(" ", "");

            // Si termina con dígito verificador, quitarlo
            if (rutLimpio.Length > 1 && !char.IsDigit(rutLimpio.Last()))
            {
                rutLimpio = rutLimpio.Substring(0, rutLimpio.Length - 1);
            }

            if (int.TryParse(rutLimpio, out int rut))
            {
                return rut;
            }

            return null;
        }

        /// Obtiene los cobros pendientes de un contribuyente específico
        public async Task<List<VCobros>> ObtenerCobrosPorContribuyente(int contribuyenteId)
        {
            return await _context.VCobros
                .Where(c => c.con_id == contribuyenteId)
                .OrderBy(c => c.cob_fecha_vencimiento)
                .ToListAsync();
        }

        /// Obtiene un cobro específico por su ID
        public async Task<VCobros?> ObtenerCobroPorId(int cobroId)
        {
            return await _context.VCobros
                .FirstOrDefaultAsync(c => c.Id == cobroId);
        }

        /// Registra un pago para un cobro específico
        public async Task<bool> RegistrarPagoAsync(int cobroId, decimal valorPagado, int recaudadorId)
        {
            try
            {
                // Verificar que el cobro existe
                var cobro = await _context.Cobros.FindAsync(cobroId);
                if (cobro == null)
                {
                    throw new InvalidOperationException("El cobro especificado no existe.");
                }

                // Verificar que el valor pagado sea positivo
                if (valorPagado <= 0)
                {
                    throw new ArgumentException("El valor a pagar debe ser mayor a cero.");
                }

                // Verificar que no exceda el valor del cobro
                if (valorPagado > cobro.cob_valor)
                {
                    throw new ArgumentException($"El valor a pagar (${valorPagado:N0}) no puede ser mayor al valor del cobro (${cobro.cob_valor:N0}).");
                }

                // Verificar pagos anteriores
                var pagosPrevios = await _context.Pagos
                    .Where(p => p.cob_id == cobroId)
                    .SumAsync(p => p.pag_valor_pagado);

                var saldoPendiente = cobro.cob_valor - pagosPrevios;

                if (valorPagado > saldoPendiente)
                {
                    throw new ArgumentException($"El valor a pagar (${valorPagado:N0}) excede el saldo pendiente (${saldoPendiente:N0}).");
                }

                // Crear el nuevo pago
                var nuevoPago = new Pagos
                {
                    cob_id = cobroId,
                    pag_fecha = DateTime.Now,
                    pag_valor_pagado = valorPagado,
                    rec_id = recaudadorId,
                    FechaRegistro = DateTime.Now
                };

                _context.Pagos.Add(nuevoPago);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al registrar pago para cobro {CobroId}", cobroId);
                throw;
            }
        }

        /// Obtiene el saldo pendiente de un cobro (valor del cobro - pagos realizados)
        public async Task<decimal> ObtenerSaldoPendienteCobro(int cobroId)
        {
            var cobro = await _context.Cobros.FindAsync(cobroId);
            if (cobro == null) return 0;

            var pagosPrevios = await _context.Pagos
                .Where(p => p.cob_id == cobroId)
                .SumAsync(p => p.pag_valor_pagado);

            return cobro.cob_valor - pagosPrevios;
        }

        /// Obtiene los pagos realizados para un cobro específico
        public async Task<List<Pagos>> ObtenerPagosPorCobro(int cobroId)
        {
            return await _context.Pagos
                .Where(p => p.cob_id == cobroId)
                .Include(p => p.Recaudador)
                .OrderByDescending(p => p.pag_fecha)
                .ToListAsync();
        }

        /// Obtiene el ID del recaudador segun en el usuario actual
        public async Task<int?> ObtenerRecaudadorPorUsuario(int usuarioId)
        {
            var recaudador = await _context.Recaudadores
                .Where(r => r.usu_id == usuarioId && r.rec_activo)
                .FirstOrDefaultAsync();

            return recaudador?.Id;
        }

        /// <summary>
        /// Obtiene el nombre completo del recaudador
        /// </summary>
        public async Task<string> ObtenerNombreRecaudador(int recaudadorId)
        {
            var recaudador = await _context.VRecaudadores
                .FirstOrDefaultAsync(r => r.Id == recaudadorId);

            return recaudador?.Nombre ?? "Recaudador no encontrado";
        }

        /// <summary>
        /// Obtiene el nombre de la compañía
        /// </summary>
        public async Task<string> ObtenerNombreCompania(int companiaId)
        {
            var compania = await _context.Companias
                .FirstOrDefaultAsync(c => c.Id == companiaId);

            return compania?.com_nombre ?? "Compañía no encontrada";
        }

        /// <summary>
        /// Obtiene un pago por su ID
        /// </summary>
        public async Task<Pagos?> ObtenerPagoPorId(int pagoId)
        {
            return await _context.Pagos
                .Include(p => p.Recaudador)
                .FirstOrDefaultAsync(p => p.Id == pagoId);
        }


        /// Guarda un comprobante de pago en la base de datos y en el sistema de archivos
        public async Task<int> GuardarComprobanteAsync(int pagoId, string nombreArchivo, byte[] contenidoArchivo, string tipoComprobante = "PDF")
        {
            try
            {
                // Verificar que el pago existe
                var pago = await _context.Pagos.FindAsync(pagoId);
                if (pago == null)
                {
                    throw new InvalidOperationException($"No se encontró el pago con ID {pagoId}");
                }

                // Crear directorio si no existe
                var directorioComprobantes = Path.Combine("wwwroot", "comprobantes", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("D2"));
                Directory.CreateDirectory(directorioComprobantes);

                // Generar nombre único para el archivo
                var extension = Path.GetExtension(nombreArchivo);
                var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivoUnico = $"{nombreSinExtension}_{pagoId}_{timestamp}{extension}";
                var rutaCompleta = Path.Combine(directorioComprobantes, nombreArchivoUnico);

                // Guardar archivo físico
                await File.WriteAllBytesAsync(rutaCompleta, contenidoArchivo);

                // Crear registro en base de datos
                var comprobante = new ComprobantesPago
                {
                    pag_id = pagoId,
                    comp_ruta_archivo = rutaCompleta.Replace("wwwroot", "").Replace("\\", "/"),
                    comp_nombre_original = nombreArchivo,
                    comp_extension = extension,
                    comp_tamaño_kb = Math.Round(contenidoArchivo.Length / 1024.0m, 2),
                    comp_tipo_comprobante = tipoComprobante,
                    comp_descripcion = $"Comprobante generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm}",
                    comp_activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.ComprobantesPago.Add(comprobante);
                await _context.SaveChangesAsync();

                _logger?.LogInformation($"Comprobante guardado exitosamente - ID: {comprobante.Id}, Pago ID: {pagoId}");
                return comprobante.Id;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al guardar comprobante para pago {pagoId}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los comprobantes asociados a un pago
        /// </summary>
        public async Task<List<ComprobantesPago>> ObtenerComprobantesPorPago(int pagoId)
        {
            return await _context.ComprobantesPago
                .Where(c => c.pag_id == pagoId && c.comp_activo)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un comprobante específico por su ID
        /// </summary>
        public async Task<ComprobantesPago?> ObtenerComprobantePorId(int comprobanteId)
        {
            return await _context.ComprobantesPago
                .Include(c => c.Pago)
                .FirstOrDefaultAsync(c => c.Id == comprobanteId && c.comp_activo);
        }

        /// <summary>
        /// Elimina lógicamente un comprobante (marca como inactivo)
        /// </summary>
        public async Task<bool> EliminarComprobanteAsync(int comprobanteId)
        {
            try
            {
                var comprobante = await _context.ComprobantesPago.FindAsync(comprobanteId);
                if (comprobante == null)
                {
                    return false;
                }

                comprobante.comp_activo = false;
                comprobante.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger?.LogInformation($"Comprobante eliminado lógicamente - ID: {comprobanteId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al eliminar comprobante {comprobanteId}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el contenido de un archivo de comprobante
        /// </summary>
        public async Task<byte[]?> ObtenerContenidoComprobante(int comprobanteId)
        {
            try
            {
                var comprobante = await ObtenerComprobantePorId(comprobanteId);
                if (comprobante == null)
                {
                    return null;
                }

                var rutaCompleta = Path.Combine("wwwroot", comprobante.comp_ruta_archivo.TrimStart('/'));

                if (File.Exists(rutaCompleta))
                {
                    return await File.ReadAllBytesAsync(rutaCompleta);
                }

                _logger?.LogWarning($"Archivo de comprobante no encontrado: {rutaCompleta}");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al obtener contenido de comprobante {comprobanteId}");
                return null;
            }
        }

        public async Task<bool> EditarPagoAsync(int pagoId, DateTime nuevaFecha)
        {
            try
            {
                var pago = await _context.Pagos.FindAsync(pagoId);
                if (pago == null)
                {
                    _logger?.LogWarning($"No se encontró el pago con ID {pagoId}");
                    return false;
                }

                pago.pag_fecha = nuevaFecha;

                await _context.SaveChangesAsync();

                _logger?.LogInformation($"Fecha del pago {pagoId} actualizada a {nuevaFecha:dd/MM/yyyy}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al editar fecha del pago {pagoId}");
                return false;
            }
        }
    }
}