using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVCVentas.Data;
using MVCVentas.Models;
using Microsoft.EntityFrameworkCore;

namespace MVCVentas.Controllers
{
    public class AccessController : Controller
    {
        private readonly MVCVentasContext _context;

        public AccessController(MVCVentasContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if(claimUser.Identity.IsAuthenticated )
            {
                return RedirectToAction("Index", "Ventas");
            }

            HttpContext.Session.Set("VMVentas", new VMVentas());

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMLogin modelLogin)
        {
            var user = _context.VMUser
                .Include(u => u.Categoria)
                .Where(u => u.Usuario == modelLogin.User && u.Password == modelLogin.Password)
                .SingleOrDefault();

            if (user != null)
            {
                if(user.Usuario.Equals(modelLogin.User) && user.Password.Equals(modelLogin.Password)) {
                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, modelLogin.User),
                    new Claim(ClaimTypes.Role, user.Categoria.Nombre),
                    new Claim("userid", user.Id_Usuario.ToString())
                };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = modelLogin.KeepLoggedIn,
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), properties);

                    return RedirectToAction("Index", "Ventas");
                }
            }

            ModelState.AddModelError(string.Empty, "Por favor, corroborar los datos ingresados.");
            return View();
        }
    }
}