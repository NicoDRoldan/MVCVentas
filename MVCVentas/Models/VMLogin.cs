namespace MVCVentas.Models
{
    public class VMLogin
    {
        public string User {  get; set; }
        public string Password { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
