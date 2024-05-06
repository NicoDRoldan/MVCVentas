namespace MVCVentas.Models
{
    public class VMCupon
    {
        public int Id_Cupon { get; set; }

        public decimal PorcentajeDto { get; set; }

        public virtual ICollection<VMCDetalle> Detalle { get; set; }
    }
}
