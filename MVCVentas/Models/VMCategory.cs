using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Categorias")]
    public class VMCategory
    {
        [Key]
        public int Id_Categoria { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Solo se aceptan letras, y no puede contener espacios.")]
        public string Nombre { get; set; }
    }
}
