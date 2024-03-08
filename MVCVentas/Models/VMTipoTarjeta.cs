using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("TiposTarjeta")]
    public class VMTipoTarjeta
    {
        [Key]
        public string CodTarjeta { get; set; }

        public string CodTipoTran { get; set; }

        [ForeignKey("CodTipoTran")]
        public virtual VMTipoTransaccion TipoTransaccion { get; set; }
    }
}
