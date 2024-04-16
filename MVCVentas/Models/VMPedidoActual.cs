using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("PedidosActuales")]
    public class VMPedidoActual
    {
        public int NumPedido { get; set; }

        [ForeignKey("NumVenta, CodComprobante, CodModulo, NumSucursal")]
        public string NumVenta { get; set; }

        [Key]
        public string CodComprobante { get; set; }

        [Key]
        public string CodModulo { get; set; }

        [Key]
        public string NumSucursal { get; set; }

        [Key]
        public int Renglon { get; set; }

        public int Id_Articulo { get; set; } //FK

        public decimal Cantidad { get; set; }

        public string? Retira { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

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
