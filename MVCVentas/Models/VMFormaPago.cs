using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("FormasPago")]
    public class VMFormaPago
    {
        [Key]
        public int Id_FormaPago { get; set; }
        public string Nombre { get; set; }
    }
}
