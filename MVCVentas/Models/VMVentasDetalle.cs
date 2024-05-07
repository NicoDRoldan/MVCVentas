namespace MVCVentas.Models
{
    public class VMVentasDetalle
    {
        public string Id_Articulo { get; set; }

        public string Detalle { get; set; }

        public string? Cantidad { get; set; }

        public string? PrecioUnitario { get; set; }

        public string? PrecioTotal { get; set; }

        public string? Id_Promo { get; set; }

        public string? AplicaPromo { get; set; }

        public string? Id_Combo { get; set; }

        public string? AplicaCombo { get; set; }

        public string? EsCupon { get; set; }

        public string? PorcentajeDescuentoCupon { get; set; }
    }
}
