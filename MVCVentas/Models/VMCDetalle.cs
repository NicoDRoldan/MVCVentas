namespace MVCVentas.Models
{
    public class VMCDetalle
    {
        public int Id_Cupon { get; set; }

        public int Id_ArticuloAsociado { get; set; }

        public int Cantidad { get; set; }

        public virtual VMArticle Articulo { get; set; }
    }
}
