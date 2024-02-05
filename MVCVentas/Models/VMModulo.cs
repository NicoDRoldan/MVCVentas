using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Modulos")]
    public class VMModulo
    {
        [Key]
        public string CodModulo { get; set; }

        public string Descripcion { get; set; }
    }
}
