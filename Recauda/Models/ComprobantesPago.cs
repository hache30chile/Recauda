using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class ComprobantesPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int pag_id { get; set; } // FK hacia Pagos

        [Required]
        [StringLength(500)]
        public string comp_ruta_archivo { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string comp_nombre_original { get; set; } = string.Empty;

        [StringLength(10)]
        public string comp_extension { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? comp_tamaño_kb { get; set; }

        [StringLength(50)]
        public string comp_tipo_comprobante { get; set; } = string.Empty; // Ej: "Recibo", "Transferencia", "Cheque"

        [StringLength(1000)]
        public string? comp_descripcion { get; set; }

        [Required]
        public bool comp_activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public DateTime? FechaModificacion { get; set; }

        // Propiedad de navegación hacia Pagos
        public virtual Pagos? Pago { get; set; }
    }
}