using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Rubros")]
    public class VMRubro
    {
        [Key]
        public int Id_Rubro { get; set; }

        public string Nombre { get; set; }

        public virtual ICollection<VMArticle> Articulos { get; set; }
    }
}