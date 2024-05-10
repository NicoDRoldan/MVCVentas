using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCVentas.Data;
using MVCVentas.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Org.BouncyCastle.Asn1.Ocsp;
using Azure;
using System.Drawing;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class CuponesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MVCVentasContext _context;

        public CuponesController(IHttpClientFactory httpClientFactory, MVCVentasContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var wsCuponesClient = _httpClientFactory.CreateClient("WSCuponesClient");
                wsCuponesClient.DefaultRequestHeaders.Accept.Clear();
                wsCuponesClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                    .MediaTypeWithQualityHeaderValue("application/json"));

                var response = await wsCuponesClient.GetAsync("api/Cupones");

                if (response.IsSuccessStatusCode)
                {
                    var cuponJson = await response.Content.ReadAsStringAsync();
                    var vMCupones = JsonConvert.DeserializeObject<List<VMCupon>>(cuponJson);
                    return View(vMCupones);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }

            TempData["Error"] = "Error al realizar la conexión.";
            return View();
        }

        public IActionResult Create()
        {
            var model = new VMCupon
            {
                ListaArticulos = _context.VMArticle
                    .Select(a => new SelectListItem { Value = a.Id_Articulo.ToString(), Text = a.Nombre })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AltaCupon(VMCupon vMCupon)
        {
            if (vMCupon.Detalle is null)
            {
                var reason = "Por favor cargar artículos";
                TempData["Error"] = reason;
                return Json(new { success = false, error = reason });
            }

            try
            {
                var wsCuponesClient = _httpClientFactory.CreateClient("WSCuponesClient");
                wsCuponesClient.DefaultRequestHeaders.Accept.Clear();
                wsCuponesClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                    .MediaTypeWithQualityHeaderValue("application/json"));

                var vMCuponJson = JsonConvert.SerializeObject(vMCupon);

                var content = new StringContent(vMCuponJson, Encoding.UTF8, "application/json");

                var response = await wsCuponesClient.PostAsync("api/Cupones/CrearCupon", content);

                if (response.IsSuccessStatusCode)
                {
                    var reason = "Cupón creado correctamente.";
                    TempData["SuccessMsg"] = reason;
                    return Json(new { success = true, message = reason });
                }
                else
                {
                    var reason = response.ReasonPhrase;
                    TempData["Error"] = reason;
                    return Json(new { success = false, error = reason });
                }

            }
            catch (Exception ex)
            {
                var reason = ex.Message;
                TempData["Error"] = reason;
                return Json(new { success = false, error = reason });
            }
        }
    }
}
