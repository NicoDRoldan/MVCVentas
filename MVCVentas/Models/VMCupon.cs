namespace MVCVentas.Models
{
    public class VMCupon
    {
        public int Id_Cupon { get; set; }

        public string? Descripcion { get; set; }

        public decimal PorcentajeDto { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public virtual ICollection<VMCDetalle> Detalle { get; set; }
    }
}
