using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Articulos")]
    public class VMArticle
    {
        [Key]
        public int Id_Articulo { get; set; }

        public string Nombre { get; set; }

        public string Rubro { get; set; }

        public bool Activo { get; set; }

        public string Descripcion { get; set; }

        public DateTime Fecha { get; set; }

    }
}
