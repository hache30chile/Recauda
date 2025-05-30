using System.ComponentModel.DataAnnotations;

namespace Recauda.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El login es obligatorio")]
        [Display(Name = "Usuario")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RecordarMe { get; set; } = false;

        public string? UrlRetorno { get; set; }
    }
}
