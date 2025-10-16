using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El login es obligatorio")]
        [StringLength(50, ErrorMessage = "El login no puede exceder los 50 caracteres")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT es obligatorio")]
        [Range(1000000, 99999999, ErrorMessage = "El RUT debe estar entre 1.000.000 y 99.999.999")]
        public int Rut { get; set; }

        [Required(ErrorMessage = "El dígito verificador es obligatorio")]
        [StringLength(1, ErrorMessage = "El dígito verificador debe ser un solo carácter")]
        [RegularExpression(@"^[0-9Kk]$", ErrorMessage = "El dígito verificador debe ser un número o K")]
        public string Dv { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100 caracteres")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int RolId { get; set; }

        public virtual Rol? Rol { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relación inversa con Recaudadores
        public virtual ICollection<Recaudador>? Recaudadores { get; set; }

        // Relación inversa con Generadores
        public virtual ICollection<Generadores>? Generadores { get; set; }

        // Propiedad calculada para mostrar RUT completo
        public string RutCompleto => $"{Rut:N0}-{Dv}";
    }
}