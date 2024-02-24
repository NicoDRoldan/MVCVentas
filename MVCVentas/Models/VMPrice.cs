using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models

{
    [Table("Precios")]
    public class VMPrice
    {
        [Key]
        public int Id_Articulo { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Fecha { get; set; }

        [ForeignKey("Id_Articulo")]
        public virtual VMArticle Articulo { get; set; }
    }
}