using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Ventas_E")]
    public class VMVentas_E
    {
        [Key]
        public string NumVenta { get; set; }

        [Key]
        public string CodComprobante { get; set; }

        [Key]
        public string CodModulo { get; set; }

        [Key]
        public string NumSucursal { get; set; } //FK

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Fecha { get; set; }

        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTime Hora { get; set; }

        public int id_FormaPago { get; set; } //FK

        public string CodCliente { get; set; } //FK

        public int id_Usuario { get; set; } //FK

        public int NumCaja { get; set; }

        // Relaciones

        public virtual VMComprobante_E Comprobante { get; set; }

        public virtual VMModulo Modulo { get; set; }

        [ForeignKey("NumSucursal")]
        public virtual VMSucursal Sucursal { get; set; }

        [ForeignKey("id_FormaPago")]
        public virtual VMFormaPago FormaPago { get; set; }

        [ForeignKey("CodCliente")]
        public virtual VMCliente Cliente { get; set; }

        [ForeignKey("id_Usuario")]
        public virtual VMUser Usuario { get; set; }

        public virtual ICollection<VMVentas_I> Ventas_I { get; set; }

        public virtual ICollection<VMVentas_D> Ventas_D { get; set; }

        public virtual ICollection<VMVentas_TipoTransaccion> VMVentas_TipoTransaccion { get; set; }
    }
}
