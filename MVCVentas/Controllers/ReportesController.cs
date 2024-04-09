using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;
using Newtonsoft.Json;

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
            var jsonTotalTransactionFilter = HttpContext.Session.GetString("TransaccionEntreFechasFiltradas");

            if(jsonTotalTransactionFilter != null)
            {
                var totalTransactionFilter = JsonConvert.DeserializeObject<List<VMReporte>>(jsonTotalTransactionFilter)
                    ?? new List<VMReporte>();

                HttpContext.Session.Remove("TransaccionEntreFechasFiltradas");

                return View(totalTransactionFilter);
            }

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
                    VentasRealizadas = vtt.Count(),
                    ImporteTotal = vtt.Sum(i => i.VentaTran.Importe)
                })
                .OrderByDescending(vtt => vtt.VentasRealizadas)
                .ToListAsync();
                
            return View(totalTransactionContext);
        }

        [HttpPost]
        public async Task<IActionResult> TotalTransaccionesEntreFechas(string fechaInicio, string fechaFin)
        {
            // Si la fecha de inicio o fin es null, vuelve a la página y muestra error.
            if (fechaInicio is null || fechaFin is null)
            {
                string referrerUrl = Request.Headers["Referer"].ToString();

                TempData["ErrorMessage"] = "Las fechas de inicio y fin son requeridas";
                return Redirect(referrerUrl);
            }

            // Convertir los string de las fechas en tipos DateTime.
            DateTime fechaInicioDate = DateTime.Parse(fechaInicio).Date;
            DateTime fechaFinDate = DateTime.Parse(fechaFin).Date;

            var totalTransactionEntreFechas = await _context.VMVentas_TipoTransaccion
                .Join(_context.VMTipoTransaccion,
                    vt => vt.CodTipoTran,
                    tt => tt.CodTipoTran,
                    (vt, tt) => new { VentaTran = vt, TipoTran = tt })
                .Join(_context.VMVentas_E,
                    vtt => new { vtt.VentaTran.NumVenta, vtt.VentaTran.NumSucursal, vtt.VentaTran.CodComprobante, vtt.VentaTran.CodModulo },
                    ve => new { ve.NumVenta, ve.NumSucursal, ve.CodComprobante, ve.CodModulo },
                    (vtt, ve) => new { VentaTran = vtt, VentasE = ve })
                .Where(v => v.VentasE.Fecha >= fechaInicioDate & v.VentasE.Fecha <= fechaFinDate)
                .GroupBy(v => new { v.VentaTran.TipoTran.Nombre, v.VentaTran.VentaTran.CodTipoTran })
                .Select(g => new VMReporte
                {
                    Nombre = g.Key.Nombre,
                    CodTipoTran = g.Key.CodTipoTran,
                    VentasRealizadas = g.Count(),
                    ImporteTotal = g.Sum(v => v.VentaTran.VentaTran.Importe)
                })
                .OrderByDescending(g => g.VentasRealizadas)
                .ToListAsync();
            
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            HttpContext.Session.SetString("TransaccionEntreFechasFiltradas",
                JsonConvert.SerializeObject(totalTransactionEntreFechas, settings));

            return RedirectToAction("ReporteTotalTransacciones", "Reportes");
        }
    }
}