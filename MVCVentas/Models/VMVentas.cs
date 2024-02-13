namespace MVCVentas.Models
{
    public class VMVentas
    {
        public VMVentas()
        {
            ArticulosEnResumenVentas = new List<VMArticle>(10);
        }

        public virtual ICollection<VMArticle> ArticulosEnResumenVentas { get; set; }

        public virtual VMFormaPago FormaPago { get; set; }

        public virtual VMCliente Cliente { get; set; }
    }
}
