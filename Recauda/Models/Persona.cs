using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Persona
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int per_rut { get; set; }

        public string? per_vrut { get; set; }
        public string? per_paterno { get; set; }
        public string? per_materno { get; set; }
        public string? per_nombres { get; set; }
        public string? sex_codigo { get; set; }
        public DateTime? per_fecnac { get; set; }
        public string? per_email { get; set; }
        public string? per_movil { get; set; }
        public string? per_calle { get; set; }
        public string? per_numero { get; set; }
        public string? per_depto { get; set; }
        public string? per_block { get; set; }
        public string? per_comuna { get; set; }
        // Relación inversa con Contribuyentes
        public virtual ICollection<Contribuyente>? Contribuyentes { get; set; }
        public virtual ICollection<Voluntarios>? Voluntarios { get; set; }
    }
}