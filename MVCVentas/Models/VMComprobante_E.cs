using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Comprobantes_E")]
    public class VMComprobante_E
    {
        [Key]
        public string CodComprobante { get; set; }

        public string Nombre { get; set; }

        public string CodModulo { get; set; }

        [ForeignKey("CodModulo")]
        public virtual VMModulo Modulo { get; set; }
    }
}
