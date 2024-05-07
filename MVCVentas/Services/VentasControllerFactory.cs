using MVCVentas.Controllers;
using MVCVentas.Data;
using MVCVentas.Interfaces;

namespace MVCVentas.Services
{
    public class VentasControllerFactory : IVentasControllerFactory
    {
        private readonly MVCVentasContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public VentasControllerFactory(MVCVentasContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public VentasController Create()
        {
            return new VentasController(_context, _httpClientFactory);
        }
    }
}
