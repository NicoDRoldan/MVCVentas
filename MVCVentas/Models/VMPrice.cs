using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models

{
    [Table("Precios")]
    public class VMPrice
    {
        [Key]
        public int Id_Articulo { get; set; }

        public decimal Precio { get; set; }

        public DateTime Fecha { get; set; }

        [ForeignKey("Id_Articulo")]
        public virtual VMArticle? Articulo { get; set; }
    }
}
