using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("TipoTransaccion")]
    public class VMTipoTransaccion
    {
        [Key]
        public string CodTipoTran { get; set; }

        public string Nombre { get; set; }

        public virtual ICollection<VMTipoTarjeta> Tarjetas { get; set; }
    }
}
