using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Stock")]
    public class VMStock
    {
        [Key]
        public int Id_Articulo { get; set; }

        public int Cantidad { get; set; }

        [ForeignKey("Id_Articulo")]
        public virtual VMArticle? Articulo {  get; set; }
    }
}
