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

        public int Id_Rubro { get; set; }

        public bool Activo { get; set; }

        public string Descripcion { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Fecha { get; set; }

        public virtual VMPrice? Precio { get; set; }

        [ForeignKey("Id_Rubro")]
        public virtual VMRubro Rubro { get; set; }
    }
}