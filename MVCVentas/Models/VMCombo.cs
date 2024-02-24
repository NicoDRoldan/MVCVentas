using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Combos")]
    public class VMCombo
    {
        [Key]
        public int Id_Combo { get; set; }

        public int Id_Articulo { get; set; }
        
        public int Id_ArticuloAgregado { get; set; }

        [ForeignKey("Id_Articulo")]
        public virtual VMArticle Articulo { get; set; }
    }
}
