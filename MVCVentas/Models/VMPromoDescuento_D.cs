using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("PromosDescuentos_D")]
    public class VMPromoDescuento_D
    {
        public int Id_Promocion { get; set; }

        public int Id_Articulo { get; set; }

        [ForeignKey("Id_Promocion")]
        [Column("Id_Promocion")]
        public virtual VMPromoDescuento_E PromoDescuento_E { get; set; }

        [ForeignKey("Id_Articulo")]
        [Column("Id_Articulo")]
        public virtual VMArticle Articulo { get; set; } 
    }
}
