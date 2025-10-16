using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recauda.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recauda.Models
{
    public class Recaudador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool rec_activo { get; set; } = true;

        [Required]
        public int usu_id { get; set; }

        [Required]
        public int com_id { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Propiedades de navegación     

        public virtual Usuario? Usuario { get; set; }

        public virtual Compania? Compania { get; set; }        

        // Relación inversa con Contribuyentes
        public virtual ICollection<Contribuyente>? Contribuyente { get; set; }
        public virtual ICollection<Pagos>? Pagos { get; set; }
        public virtual ICollection<PagosAnulados>? PagosAnulados { get; set; }       

    }



    }
