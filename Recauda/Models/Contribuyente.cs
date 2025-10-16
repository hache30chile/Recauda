using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Contribuyente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("per_rut")] // Especificar nombre exacto de columna
        public int per_rut { get; set; }

        [Required]
        [Column("mdc_id")]
        public int mdc_id { get; set; }

        [Required]
        [Column("con_valor_aporte", TypeName = "decimal(12,0)")]
        public decimal con_valor_aporte { get; set; }

        [Required]
        [StringLength(20)]
        [Column("con_periodicidad_cobro")]
        public string con_periodicidad_cobro { get; set; } = "Mensual"; // Valor por defecto

        [Required]
        [Column("con_dia_del_cargo")]
        public int con_dia_del_cargo { get; set; }

        [Required]
        [Column("con_fecha_inicio")]
        public DateTime con_fecha_inicio { get; set; }

        [Column("con_fecha_fin")]
        public DateTime? con_fecha_fin { get; set; } // Nullable importante

        [Required]
        [Column("rec_id")]
        public int rec_id { get; set; }

        [Required]
        [Column("con_activo")]
        public bool con_activo { get; set; } = true;

        [Required]
        [Column("com_id")]
        public int com_id { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Propiedades de navegación (no se mapean a columnas)
        [NotMapped]
        public virtual Persona? Persona { get; set; }

        [NotMapped]
        public virtual MotivoDeCobro? MotivoCobro { get; set; }

        [NotMapped]
        public virtual Recaudador? Recaudador { get; set; }

        [NotMapped]
        public virtual Compania? Compania { get; set; }

        [NotMapped]
        public virtual ICollection<Cobros>? Cobros { get; set; }
    }
}