using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    [Authorize]
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
            var reportTransactionContext = _context.VMVentas_TipoTransaccion
                .Include(v => v.Ventas_E)
                .Include(v => v.VMTipoTransaccion)
                .OrderByDescending(ve => ve.Ventas_E.Fecha)
                .ThenByDescending(ve => ve.Ventas_E.Hora);

            return View(await reportTransactionContext.ToListAsync());
        }

        public async Task<IActionResult> ReporteTotalTransacciones()
        {
            var totalTransactionContext = await _context.VMVentas_TipoTransaccion
                .Join(_context.VMTipoTransaccion,
                    vt => vt.CodTipoTran,
                    tt => tt.CodTipoTran,
                    (vt, tt) => new { VentaTran = vt, TipoTran = tt })
                .GroupBy(vtt => new { vtt.TipoTran.Nombre, vtt.VentaTran.CodTipoTran })
                .Select(vtt => new VMReporte
                {
                    Nombre = vtt.Key.Nombre,
                    CodTipoTran = vtt.Key.CodTipoTran,
                    VentasRealizadas = vtt.Count()
                })
                .OrderByDescending(vtt => vtt.VentasRealizadas)
                .ToListAsync();
                
            return View(totalTransactionContext);
        }
    }
}