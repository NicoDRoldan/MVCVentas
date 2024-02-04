using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("TiposPromosDescuentos")]
    public class VMTipoPromoDescuento
    {
        [Key]
        public int Id_Tipo { get; set; }

        public string Descripcion { get; set; }
    }
}
