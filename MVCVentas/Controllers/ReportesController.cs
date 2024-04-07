using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;

namespace MVCVentas.Controllers
{
    public class ReportesController : Controller
    {
        private readonly MVCVentasContext _context;

        public ReportesController(MVCVentasContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ReporteTipoTransaccion()
        {
            var reporteContext = _context.VMVentas_TipoTransaccion
                .Include(v => v.Ventas_E)
                .Include(v => v.VMTipoTransaccion)
                .OrderByDescending(ve => ve.Ventas_E.Fecha)
                .ThenByDescending(ve => ve.Ventas_E.Hora);

            return View(await reporteContext.ToListAsync());
        }
    }
}
