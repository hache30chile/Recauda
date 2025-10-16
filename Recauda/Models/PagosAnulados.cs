using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class PagosAnulados
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int pag_id { get; set; }

        [Required]
        public DateTime caa_fecha_anulacion { get; set; }

        [Required]
        public int anu_id { get; set; }

        [Required]
        public int rec_id { get; set; }

        // Propiedades de navegación
        public virtual Pagos? Pago { get; set; }
        public virtual Anulaciones? Anulacion { get; set; }
        public virtual Recaudador? Recaudador { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}