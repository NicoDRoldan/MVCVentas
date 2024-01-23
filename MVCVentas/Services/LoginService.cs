using System.Security.Claims;

namespace MVCVentas.Services
{
    public class LoginService : ILoginService
    {
        public readonly IHttpContextAccessor _httpContextAccessor;

        public LoginService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string ObtenerNombreUsuarioActual()
        {
            var user = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return user;
        }
    }
}
