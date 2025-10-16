using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class VContribuyente
    {
        public int Id { get; set; }     
        public int per_rut { get; set; }
        public string per_vrut { get; set; }
        public string per_rut_completo { get; set; }
        public string per_paterno { get; set; }
        public string per_materno { get; set; }
        public string per_nombres { get; set; }
        public string per_nombre_completo { get; set; }
        public string sex_codigo { get; set; }
        public DateTime? per_fecnac { get; set; }
        public string? per_email { get; set; }
        public string? per_movil { get; set; }
        public string? per_calle { get; set; }
        public string? per_numero { get; set; }
        public string? per_depto { get; set; }
        public string? per_block { get; set; }
        public string? per_comuna { get; set; }
        public string? per_direccion_completa { get; set; }
        public int com_id { get; set; }
        public string? com_nombre { get; set; }        
        public int mdc_id { get; set; }
        public string? mdc_nombre { get; set; }        
        [Column(TypeName = "decimal(12,0)")]
        public decimal con_valor_aporte { get; set; }        
        [StringLength(20)]  
        public string con_periodicidad_cobro { get; set; }
        public int con_dia_del_cargo { get; set; }        
        public DateTime con_fecha_inicio { get; set; }
        public DateTime? con_fecha_fin { get; set; }        
        public int rec_id { get; set; }        
        public bool con_activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
    }
}