using System.ComponentModel.DataAnnotations;

namespace Recauda.Models
{
    public class FormasDeRecaudacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string fdr_nombre { get; set; } = string.Empty;
    }
}