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

        public string Nombre { get; set; }
    }
}
