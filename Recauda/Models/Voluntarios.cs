using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class Voluntarios
    {
        [Key]
        public int id { get; set; }

        [Required]
        public int per_rut { get; set; }

        [Required]
        public int com_id { get; set; }

        [Required]
        public string vol_estado { get; set; } = string.Empty;

        // Propiedades de navegación
        public virtual Persona? Persona { get; set; }
        public virtual Compania? Compania { get; set; }
    }
}