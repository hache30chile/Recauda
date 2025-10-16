using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class CobrosAnulados
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int cob_id { get; set; }

        [Required]
        public DateTime caa_fecha_anulacion { get; set; }

        [Required]
        public int anu_id { get; set; }

        [Required]
        public int gen_id { get; set; }

        // Propiedades de navegación
        public virtual Cobros? Cobro { get; set; }
        public virtual Anulaciones? Anulacion { get; set; }
        public virtual Generadores? Generador { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}