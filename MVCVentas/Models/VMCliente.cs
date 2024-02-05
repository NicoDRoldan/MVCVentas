using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    [Table("Clientes")]
    public class VMCliente
    {
        [Key]
        public string CodCliente { get; set; }

        [RegularExpression("^[0-9]{2}-[0-9]{8}-[0-9]{1}$", ErrorMessage = "El CUIT debe tener el formato 00-00000000-0")]
        public string CUIT { get; set; }

        public string RazonSocial { get; set; }

        public string? Nombre { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string Direccion { get; set; }

        public string CodProvincia { get; set; }

        public string CodLocalidad { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaAlta { get; set; }

        // Relaciones/Navegación:

        [ForeignKey("CodProvincia")]
        public VMProvincia Provincia { get; set; }

        [ForeignKey("CodLocalidad")]
        public VMLocalidad Localidad { get; set; }
    }
}
