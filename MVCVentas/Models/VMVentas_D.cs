using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Ventas_D")]
    public class VMVentas_D
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
        public int Renglon { get; set; }

        public int Id_Articulo { get; set; } //FK

        public decimal Cantidad { get; set; }

        public string Detalle { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal PrecioTotal { get; set; }

        // Relaciones

        [ForeignKey("Id_Articulo")]
        public virtual VMArticle Articulo { get; set; }

        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public virtual VMVentas_E Ventas_E { get; set; }

        public virtual VMComprobante_E Comprobante { get; set; }

        public virtual VMModulo Modulo { get; set; }

        public virtual VMSucursal Sucursal { get; set; }
    }
}
