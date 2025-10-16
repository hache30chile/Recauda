using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Cobros
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime cob_fecha_emision { get; set; }

        [Required]
        public DateTime cob_fecha_vencimiento { get; set; }

        [Required]
        public int mdc_id { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal cob_valor { get; set; }

        [Required]
        public int con_id { get; set; }

        [Required]
        public int gen_id { get; set; }

        [Required]
        public int com_id { get; set; }

        // Propiedades de navegación
        public virtual MotivoDeCobro? MotivoCobro { get; set; }
        public virtual Contribuyente? Contribuyente { get; set; }
        public virtual Generadores? Generador { get; set; }
        public virtual Compania? Compania { get; set; }
        public virtual ICollection<Pagos>? Pagos { get; set; }
        public virtual ICollection<CobrosAnulados>? CobrosAnulados { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}