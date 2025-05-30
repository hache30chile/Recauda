using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El login es obligatorio")]
        [StringLength(50, ErrorMessage = "El login no puede exceder los 50 caracteres")]
        [Display(Name = "Login de Usuario")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT es obligatorio")]
        [Range(1000000, 99999999, ErrorMessage = "El RUT debe estar entre 1.000.000 y 99.999.999")]
        [Display(Name = "RUT")]
        public int Rut { get; set; }

        [Required(ErrorMessage = "El dígito verificador es obligatorio")]
        [StringLength(1, ErrorMessage = "El dígito verificador debe ser un solo carácter")]
        [RegularExpression(@"^[0-9Kk]$", ErrorMessage = "El dígito verificador debe ser un número o K")]
        [Display(Name = "Dígito Verificador")]
        public string Dv { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100 caracteres")]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public int RolId { get; set; }

        [Display(Name = "Rol Asignado")]
        public virtual Rol? Rol { get; set; } // Relación de navegación

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Usuario Activo")]
        public bool Activo { get; set; } = true;

        // Propiedad calculada para mostrar RUT completo
        [Display(Name = "RUT Completo")]
        public string RutCompleto => $"{Rut:N0}-{Dv}";
    }
}