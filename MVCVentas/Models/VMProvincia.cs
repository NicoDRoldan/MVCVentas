using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Provincias")]
    public class VMProvincia
    {
        [Key]   
        public string CodProvincia { get; set; }

        public string Nombre { get; set; }
    }
}
