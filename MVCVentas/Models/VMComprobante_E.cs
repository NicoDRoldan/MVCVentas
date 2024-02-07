using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Comprobantes_E")]
    public class VMComprobante_E
    {
        [Key]
        public string CodComprobante { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        public string Nombre { get; set; }

        [Key]
        public string CodModulo { get; set; }

        // Relaciones:

        [ForeignKey("CodModulo")]
        public virtual VMModulo Modulo { get; set; }
    }
}
