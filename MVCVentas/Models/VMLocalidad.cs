using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Localidades")]
    public class VMLocalidad
    {
        [Key]
        public string CodLocalidad { get; set; }

        public string Nombre { get; set; }
    }
}
