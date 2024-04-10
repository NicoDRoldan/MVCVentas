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

        #region Reporte Tipo Transacción

        public async Task<IActionResult> ReporteTipoTransaccion()
        {
            var jsonTransactionFilter = HttpContext.Session.GetString("TipoTransaccionFilter");

            if (jsonTransactionFilter != null)
            {
                var transactionFilter = JsonConvert.DeserializeObject<List<VMVentas_TipoTransaccion>>(jsonTransactionFilter)
                    ?? new List<VMVentas_TipoTransaccion>();

                HttpContext.Session.Remove("TipoTransaccionFilter");

                return View(transactionFilter);
            }

            var reportTransactionContext = _context.VMVentas_TipoTransaccion
                .Include(v => v.Ventas_E)
                .Include(v => v.VMTipoTransaccion)
                .OrderByDescending(ve => ve.Ventas_E.Fecha)
                .ThenByDescending(ve => ve.Ventas_E.Hora);

            return View(await reportTransactionContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> TipoTransaccionEntreFechas(string fechaInicio, string fechaFin)
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

            var tipoTransaccionEntreFechas = await _context.VMVentas_TipoTransaccion
                .Include(v => v.Ventas_E)
                .Include(v => v.VMTipoTransaccion)
                .Where(ve => ve.Ventas_E.Fecha >= fechaInicioDate && ve.Ventas_E.Fecha <= fechaFinDate)
                .OrderByDescending(ve => ve.Ventas_E.Fecha)
                .ThenByDescending(ve => ve.Ventas_E.Hora)
                .ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            HttpContext.Session.SetString("TipoTransaccionFilter",
                JsonConvert.SerializeObject(tipoTransaccionEntreFechas, settings));

            return RedirectToAction("ReporteTipoTransaccion", "Reportes");
        }

        #endregion

        #region Reporte Total Transacciones

        public async Task<IActionResult> ReporteTotalTransacciones()
        {
            var jsonTotalTransactionFilter = HttpContext.Session.GetString("TotalTransactionFilter");

            if(jsonTotalTransactionFilter != null)
            {
                var totalTransactionFilter = JsonConvert.DeserializeObject<List<VMReporte>>(jsonTotalTransactionFilter)
                    ?? new List<VMReporte>();

                HttpContext.Session.Remove("TotalTransactionFilter");

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

            HttpContext.Session.SetString("TotalTransactionFilter",
                JsonConvert.SerializeObject(totalTransactionEntreFechas, settings));

            return RedirectToAction("ReporteTotalTransacciones", "Reportes");
        }

        #endregion

        #region Reporte Total por Artículos

        public async Task<IActionResult> ReportTotalByArticles()
        {
            var totalBySrticles = await _context.VMVentas_D
                .Include(v => v.Articulo)
                .GroupBy(v => new { v.Id_Articulo, v.Articulo.Nombre })
                .Select (g => new VMReporte
                {
                    Id_Articulo = g.Key.Id_Articulo,
                    Nombre = g.Key.Nombre,
                    TotalVendido = g.Sum(v => v.Cantidad),
                    ImporteTotal = g.Sum(v => v.PrecioTotal)
                })
                .OrderByDescending(g => g.TotalVendido)
                .ToListAsync();

            return View(totalBySrticles);
        }

        #endregion

        #region Venta Total por Día

        public async Task<IActionResult> VentaTotalPorDia()
        {
            var jsonTotalPorDiaFilter = HttpContext.Session.GetString("TotalPorDiaFiltro");

            if (jsonTotalPorDiaFilter != null)
            {
                var totalPorDiaFilter = JsonConvert.DeserializeObject<List<VMReporte>>(jsonTotalPorDiaFilter)
                    ?? new List<VMReporte>();

                HttpContext.Session.Remove("TotalPorDiaFiltro");

                return View(totalPorDiaFilter);
            }

            var totalPorDia = new List<VMReporte>();

            return View(totalPorDia);
        }

        public async Task<IActionResult> FiltroVentsTotalPorDia(string fechaInicio, string fechaFin)
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

            var totalPorDia = await _context.VMVentas_E
                .Join(_context.VMVentas_I,
                    ve => new { ve.NumVenta, ve.NumSucursal, ve.CodComprobante, ve.CodModulo },
                    vi => new { vi.NumVenta, vi.NumSucursal, vi.CodComprobante, vi.CodModulo },
                    (ve, vi) => new { VentasE = ve, VentasI = vi })
                .Where(vi => vi.VentasI.CodConcepto == "TOTAL" 
                    && vi.VentasE.Fecha >= fechaInicioDate && vi.VentasE.Fecha <= fechaFinDate)
                .GroupBy(v => new { v.VentasE.Fecha })
                .Select(v => new VMReporte
                {
                    Fecha = v.Key.Fecha,
                    ImporteTotal = v.Sum(vi => vi.VentasI.Importe)
                })
                .ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            HttpContext.Session.SetString("TotalPorDiaFiltro",
                JsonConvert.SerializeObject(totalPorDia, settings));

            return RedirectToAction("VentaTotalPorDia", "Reportes");
        }

        #endregion
    }
}