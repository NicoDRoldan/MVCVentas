using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCVentas.Models;
using Newtonsoft.Json;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class CuponesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CuponesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
    }
}
