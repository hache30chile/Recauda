using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Generadores
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int usu_id { get; set; }

        [Required]
        public bool gen_activo { get; set; } = true;

        [Required]
        public int com_id { get; set; }

        // Propiedades de navegación
        public virtual Usuario? Usuario { get; set; }
        public virtual Compania? Compania { get; set; }
        public virtual ICollection<Cobros>? Cobros { get; set; }
        public virtual ICollection<CobrosAnulados>? CobrosAnulados { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}