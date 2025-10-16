using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class Rol
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
