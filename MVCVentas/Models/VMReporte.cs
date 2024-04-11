namespace MVCVentas.Models
{
    public class VMReporte
    {
        public int Id_Articulo { get; set; }

        public string Nombre { get; set; }

        public string CodTipoTran { get; set; }

        public int VentasRealizadas { get; set; }

        public decimal TotalVendido { get; set; }

        public decimal ImporteTotal { get; set; }

        public DateTime Fecha { get; set; }
    }
}
