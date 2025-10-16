using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    /// <summary>
    /// Vista que combina información de Cobros, Contribuyente, Persona y otros datos relacionados
    /// </summary>
    public class VCobros
    {
        [Key]
        public int Id { get; set; }

        // Información del cobro
        public DateTime cob_fecha_emision { get; set; }
        public DateTime cob_fecha_vencimiento { get; set; }
        public decimal cob_valor { get; set; }

        // Información del contribuyente
        public int con_id { get; set; }
        public string con_periodicidad_cobro { get; set; } = string.Empty;
        public int con_dia_del_cargo { get; set; }
        public bool con_activo { get; set; }

        // Información de la persona
        public int per_rut { get; set; }
        public string? per_vrut { get; set; }
        public string? per_nombres { get; set; }
        public string? per_paterno { get; set; }
        public string? per_materno { get; set; }
        public string? per_email { get; set; }
        public string? per_movil { get; set; }

        // Información del motivo de cobro
        public int mdc_id { get; set; }
        public string? mdc_nombre { get; set; }

        // Información de la compañía
        public int com_id { get; set; }
        public string? com_nombre { get; set; }

        // Información del generador
        public int gen_id { get; set; }
        public string? gen_nombre { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string per_nombre_completo =>
            $"{per_nombres} {per_paterno} {per_materno}".Trim();

        [NotMapped]
        public string per_rut_formateado =>
            per_vrut ?? $"{per_rut:N0}";

        [NotMapped]
        public bool esta_vencido =>
            DateTime.Now.Date > cob_fecha_vencimiento.Date;

        [NotMapped]
        public int dias_para_vencimiento =>
            (cob_fecha_vencimiento.Date - DateTime.Now.Date).Days;
    }
}