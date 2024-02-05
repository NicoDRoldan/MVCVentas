using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Comprobantes_E")]
    public class VMComprobante_E
    {
        [Key]
        [Column(Order = 0)]
        [Required(ErrorMessage = "El campo Código de Comprobante es obligatorio")]
        public string CodComprobante { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        public string Nombre { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CodModulo { get; set; }

        [ForeignKey("CodModulo")]
        public virtual VMModulo Modulo { get; set; }
    }
}
