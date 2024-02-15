using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Conceptos")]
    public class VMConcepto
    {
        [Key]
        public string CodConcepto { get; set; }

        public string Descripcion { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        public decimal Porcentaje { get; set; }

        public virtual ICollection<VMVentas_I> Ventas_I { get; set; }
    }
}
