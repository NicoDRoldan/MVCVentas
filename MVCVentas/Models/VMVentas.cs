namespace MVCVentas.Models
{
    public class VMVentas
    {
        public VMVentas()
        {
            ArticulosEnResumenVentas = new List<VMArticle>();
        }

        // Ventas_E

        public string NumVenta { get; set; }

        public string CodComprobante { get; set; }

        public virtual VMComprobante_E Comprobante_E { get; set; }

        public virtual VMComprobante_N Comprobante_N { get; set; }

        public string CodModulo { get; set; }

        public virtual VMModulo Modulo { get; set; }

        public string NumSucursal { get; set; }

        public virtual VMSucursal Sucursal { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime Hora { get; set; }

        public int Id_FormaPago { get; set; }

        public virtual VMFormaPago FormaPago { get; set; }

        public virtual List<int> FormasPagos { get; set; }

        public string CodCliente { get; set; }

        public virtual VMCliente Cliente { get; set; }

        public int Id_Usuario { get; set; }

        public virtual VMUser Usuario { get; set; }

        public int NumCaja { get; set; }

        // Ventas_D

        public virtual List<VMVentasDetalle> detallesventa { get; set; }

        public int Renglon { get; set; }

        public virtual ICollection<VMArticle> ArticulosEnResumenVentas { get; set; }

        public int Id_Articulo { get; set; }

        public virtual VMArticle Articulo { get; set; }

        public int Cantidad { get; set; }

        public string Detalle { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal PrecioTotal { get; set; }

        // Ventas_I

        public virtual VMConcepto Concepto { get; set; }

        public decimal Importe { get; set; }

        public decimal Descuento { get; set; }

        // Ventas_TipoTransaccion

        public List<VMVentas_TipoTransaccion> Ventas_TipoTransacciones { get; set; }

        public VMVentas_TipoTransaccion Ventas_TipoTransaccion { get; set; }

        public VMTipoTarjeta VMTipoTarjeta { get; set; }

        public VMTipoTransaccion VMTipoTransaccion { get; set; }

        public string CodTarjeta { get; set; }

        public string CodTipoTran { get; set; }

        public string NumTransaccion { get; set; }

        public string Pago { get; set; } // Monto con el que pagó el cliente.

        public string Vuelto { get; set; } // Vuelto al cliente.

        // Varios:

        public string? Retira { get; set; }
    }
}