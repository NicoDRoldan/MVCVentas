using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("TipoFacturas")]
    public class VMTipoFactura
    {
        [Key]
        public int Id_TipoFactura { get; set; }

        public string Nombre { get; set; }
    }
}