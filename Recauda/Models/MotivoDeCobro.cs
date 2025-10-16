using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class MotivoDeCobro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string mdc_nombre { get; set; } = string.Empty;

        [Required]
        public bool mdc_activo { get; set; } = true;

        // Relación inversa con Contribuyentes
        public virtual ICollection<Contribuyente>? Contribuyente { get; set; }

        // Relación inversa con Cobros
        public virtual ICollection<Cobros>? Cobros { get; set; }
    }
}