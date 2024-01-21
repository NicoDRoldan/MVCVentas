using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Usuarios")]
    public class VMUser
    {
        [Key]
        public int Id_Usuario { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public bool Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        
        public int Id_Categoria { get; set; }
        [ForeignKey("Id_Categoria")]
        public virtual VMCategory? Categoria { get; set; }
    }
}
