using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Pagos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int cob_id { get; set; }

        [Required]
        public DateTime pag_fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal pag_valor_pagado { get; set; }

        [Required]
        public int rec_id { get; set; }

        // Propiedades de navegación existentes
        public virtual Cobros? Cobro { get; set; }
        public virtual Recaudador? Recaudador { get; set; }
        public virtual ICollection<PagosAnulados>? PagosAnulados { get; set; }

        // Nueva propiedad de navegación para comprobantes
        public virtual ICollection<ComprobantesPago>? ComprobantesPago { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}