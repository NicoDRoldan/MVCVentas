using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Usuarios")]
    public class VMUser
    {
        [Key]
        public int Id_Usuario { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Solo se aceptan letras y números, y no puede contener espacios.")]
        public string Usuario { get; set; }

        [MinLength(6, ErrorMessage = "La longitud mínima es 6 caracteres.")]
        [MaxLength(20, ErrorMessage = "La longitud máxima es 20 caracteres.")]
        public string Password { get; set; }
        public bool Estado { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Fecha { get; set; }

        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Solo se aceptan letras, y no puede contener espacios.")]
        public string Nombre { get; set; }

        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Solo se aceptan letras, y no puede contener espacios.")]
        public string Apellido { get; set; }
        
        public int Id_Categoria { get; set; }

        [ForeignKey("Id_Categoria")]
        public virtual VMCategory Categoria { get; set; }
    }
}
