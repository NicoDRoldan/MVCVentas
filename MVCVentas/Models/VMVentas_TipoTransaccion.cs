using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Ventas_TipoTransaccion")]
    public class VMVentas_TipoTransaccion
    {
        public string CodTipoTran { get; set; }

        [Key]
        public string NumTransaccion { get; set; }

        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public string NumVenta { get; set; }

        [Key]
        public string CodComprobante { get; set; }

        [Key]
        public string CodModulo { get; set; }

        [Key]
        public string NumSucursal { get; set; }

        public decimal Importe { get; set; }

        // Relaciones

        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public virtual VMVentas_E Ventas_E { get; set; }

        public virtual VMComprobante_E Comprobante { get; set; }

        public virtual VMModulo Modulo { get; set; }

        public virtual VMSucursal Sucursal { get; set; }

        [ForeignKey("CodTipoTran")]
        public virtual VMTipoTransaccion VMTipoTransaccion { get; set; }
    }
}
