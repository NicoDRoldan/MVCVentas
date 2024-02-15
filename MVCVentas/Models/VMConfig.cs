using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Config")]
    public class VMConfig
    {
        [Key]
        public string Codigo_Config { get; set; }

        public string Descripcion_Config { get; set; }

        public string Valor_Config { get; set; }

        public DateTime Fecha_Creacion { get; set; }
    }
}
