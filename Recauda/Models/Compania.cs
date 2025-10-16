using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Compania
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        public string com_nombre { get; set; } = string.Empty;

        // Relación inversa con Recaudadores
        public virtual ICollection<Recaudador>? Recaudadores { get; set; }
        public virtual ICollection<Contribuyente>? Contribuyentes { get; set; }
        public virtual ICollection<Voluntarios>? Voluntarios { get; set; }
        public virtual ICollection<Generadores>? Generadores { get; set; }
        public virtual ICollection<Cobros>? Cobros { get; set; }
    }
}