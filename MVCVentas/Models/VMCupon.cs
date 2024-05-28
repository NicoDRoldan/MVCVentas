using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    public class VMCupon
    {
        public int Id_Cupon { get; set; }

        [Required]
        public string? Descripcion { get; set; }

        [Required]
        public decimal PorcentajeDto { get; set; }

        [Required]
        public DateTime? FechaInicio { get; set; }

        [Required]
        public DateTime? FechaFin { get; set; }

        [Required]
        public string TipoCupon { get; set; }

        public virtual ICollection<VMCDetalle> Detalle { get; set; }

        [NotMapped]
        public List<SelectListItem> ListaArticulos { get; set; }

        public VMCupon()
        {
            ListaArticulos = new List<SelectListItem>();
        }
    }
}
