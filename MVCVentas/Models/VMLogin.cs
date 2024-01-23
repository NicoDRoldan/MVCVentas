using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCVentas.Models
{
    public class VMLogin
    {
        [Required]
        public string User {  get; set; }
        [Required]
        public string Password { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
