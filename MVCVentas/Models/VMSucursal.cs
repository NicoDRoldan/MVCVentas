using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Sucursales")]
    public class VMSucursal
    {
        [Key]
        public string NumSucursal { get; set; }

        public int Id_TipoFactura { get; set; }

        [ForeignKey("Id_TipoFactura")]
        public virtual VMTipoFactura TipoFactura { get; set; }

        public virtual ICollection<VMComprobante_N> Comprobante_N { get; set; }
    }
}