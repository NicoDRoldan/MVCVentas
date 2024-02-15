namespace MVCVentas.Models
{
    public class VMVentaImporte
    {
        public virtual VMConcepto Concepto { get; set; }

        public string CodConcepto { get; set; }

        public string Importe { get; set; }

        public string Descuento { get; set; }

        /*
         * SubTotal = Cantidad * Precio
         * Descuento = SubTotal * (Porcentaje de Descuento / 100)
         * Neto = (SubTotal - Descuento) / IVA -> (1,21)
         * IVA = SubTotal - Descuento - Neto
         * Total Neto + IVA
         */
    }
}
