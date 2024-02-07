using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Ventas_I")]
    public class VMVentas_I
    {
        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public string NumVenta { get; set; }

        [Key]
        public string CodComprobante { get; set; }

        [Key]
        public string CodModulo { get; set; }

        [Key]
        public string NumSucursal { get; set; } //FK

        [Key]
        public string CodConcepto { get; set; } //FK

        public decimal Importe { get; set; }

        public decimal Descuento { get; set; }

        // Relaciones

        [ForeignKey("CodConcepto")]
        public virtual VMConcepto Concepto { get; set; }

        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public virtual VMVentas_E Ventas_E { get; set; }

        public virtual VMComprobante_E Comprobante { get; set; }

        public virtual VMModulo Modulo { get; set; }

        public virtual VMSucursal Sucursal { get; set; }
    }
}
