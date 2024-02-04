using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("PromosDescuentos_E")]
    public class VMPromoDescuento_E
    {
        [Key]
        public int Id_Promocion { get; set; }

        public string Nombre { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        public decimal Porcentaje { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaInicio { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaFin { get; set; }

        public int Id_Tipo { get; set; }

        [ForeignKey("Id_Tipo")]
        public virtual VMTipoPromoDescuento TipoPromoDescuento { get; set; }

        [NotMapped]
        public List<int> ArticulosSeleccionados { get; set; }

        [NotMapped]
        public List<SelectListItem> ListaArticulos { get; set; }

        public virtual ICollection<VMPromoDescuento_D> ListPromoDescuento_D { get; set; }

        public VMPromoDescuento_E()
        {
            ArticulosSeleccionados = new List<int>();
            ListaArticulos = new List<SelectListItem>();
        }
    }
}
