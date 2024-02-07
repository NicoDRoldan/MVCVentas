using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Comprobantes_N")]
    public class VMComprobante_N
    {
        [Key]
        public string CodComprobante { get; set; }

        [Key]
        public string CodModulo { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal NumComprobante { get; set; }

        [Key]
        public string NumSucursal { get; set; }

        // Relaciones:

        [ForeignKey("CodComprobante, CodModulo")]
        public virtual VMComprobante_E Comprobante_E { get; set; }

        [ForeignKey("CodModulo")]
        public virtual VMModulo Modulo { get; set; }

        [ForeignKey("NumSucursal")]
        public virtual VMSucursal Sucursal { get; set; }
    }
}
