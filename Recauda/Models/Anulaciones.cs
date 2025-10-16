using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class Anulaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string anu_descripcion { get; set; } = string.Empty;

        [Required]
        public bool anu_activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        public virtual ICollection<CobrosAnulados>? CobrosAnulados { get; set; }
        public virtual ICollection<PagosAnulados>? PagosAnulados { get; set; }
    }
}