using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Modulos")]
    public class VMModulo
    {
        [Key]
        [Required(ErrorMessage = "El campo es obligatorio")]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Solo se aceptan letras, y no puede contener espacios.")]
        public string CodModulo { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio")]
        public string Descripcion { get; set; }

        public virtual ICollection<VMComprobante_N> Comprobantes_N { get; set; }
    }
}
