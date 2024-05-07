using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Interfaces;
using MVCVentas.Models;
using Newtonsoft.Json;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class Ventas_EController : Controller
    {
        private readonly MVCVentasContext _context;
        private readonly IVentasControllerFactory _ventasControllerFactory;

        public Ventas_EController(MVCVentasContext context, IVentasControllerFactory ventasControllerFactory)
        {
            _context = context;
            _ventasControllerFactory = ventasControllerFactory;
        }

        // GET: Ventas_E
        public async Task<IActionResult> Index()
        {
            var jsonVentasFiltradas = HttpContext.Session.GetString("VentasFiltradas");

            if(jsonVentasFiltradas != null)
            {
                var ventasFiltradas = JsonConvert.DeserializeObject<List<VMVentas_E>>(jsonVentasFiltradas) ?? new List<VMVentas_E>();

                HttpContext.Session.Remove("VentasFiltradas");

                return View(ventasFiltradas);
            }
            else
            {
                var mVCVentasContext = _context.VMVentas_E
                    .Include(v => v.Cliente)
                    .Include(v => v.Comprobante)
                    .Include(v => v.FormaPago)
                    .Include(v => v.Modulo)
                    .Include(v => v.Sucursal)
                    .Include(v => v.Usuario)
                    .Include(v => v.Ventas_D)
                    .Include(v => v.Ventas_I)
                .OrderByDescending(v => v.Fecha)
                .ThenByDescending(v => v.Hora);

                return View(await mVCVentasContext.ToListAsync());
            }
        }

        // GET: Ventas_E/Details/5
        public async Task<IActionResult> Details(string numVenta, string codComprobante, string codModulo, string numSucursal)
        {
            if (numVenta == null || codComprobante == null || codModulo == null || numSucursal == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E
                .Where(ve => ve.NumVenta == numVenta
                        && ve.CodComprobante == codComprobante
                        && ve.CodModulo == codModulo
                        && ve.NumSucursal == numSucursal
                )
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .Include(v => v.Ventas_D)
                .Include(v => v.Ventas_I)
                .FirstOrDefaultAsync();

            ViewData["Ventas_E"] = vMVentas_E;

            // Traer los detalles de la venta:
            var vMVentas_D = await _context.VMVentas_D
                .Where(vd => vd.NumVenta == numVenta
                    && vd.CodComprobante == codComprobante
                    && vd.CodModulo == codModulo
                    && vd.NumSucursal == numSucursal
                )
                .Include(vd => vd.Articulo)
                .ToListAsync();

            ViewData["Ventas_D"] = vMVentas_D;

            // Traer los importes de la venta:
            var vMVentas_I = await _context.VMVentas_I
                .Where(vi => vi.NumVenta == numVenta
                        && vi.CodComprobante == codComprobante
                        && vi.CodModulo == codModulo
                        && vi.NumSucursal == numSucursal
               )
                .Include(vi => vi.Concepto)
                .ToListAsync();

            ViewData["Ventas_I"] = vMVentas_I;

            if (vMVentas_E == null)
            {
                return NotFound();
            }

            return View(vMVentas_E);
        }

        [HttpPost]
        public async Task<IActionResult> LlamarAReimpresion(string numVenta, string codComprobante, string codModulo, string numSucursal)
        {
            var vMVentas_E = await _context.VMVentas_E
                .Where(ve => ve.NumVenta == numVenta
                    && ve.CodComprobante == codComprobante
                    && ve.CodModulo == codModulo
                    && ve.NumSucursal == numSucursal
            )
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .Include(v => v.Ventas_D)
                .Include(v => v.Ventas_I)
                .FirstOrDefaultAsync();

            var vMVentas_D = await _context.VMVentas_D
                .Where(vd => vd.NumVenta == numVenta
                    && vd.CodComprobante == codComprobante
                    && vd.CodModulo == codModulo
                    && vd.NumSucursal == numSucursal
                )
                .Include(vd => vd.Articulo)
                .ToListAsync();

            var vMVentas_I = await _context.VMVentas_I
                .Where(vi => vi.NumVenta == numVenta
                    && vi.CodComprobante == codComprobante
                    && vi.CodModulo == codModulo
                    && vi.NumSucursal == numSucursal
                )
                .Include(vi => vi.Concepto)
                .ToListAsync();

            var listaVentaTipoTransaccion = await _context.VMVentas_TipoTransaccion
                .Join(_context.VMTipoTransaccion,
                    vt => vt.CodTipoTran,
                    tt => tt.CodTipoTran,
                    (vt, tt) => new { ventaTipoTransaccion = vt, tipoTransaccion = tt })
                    .Where(v => v.ventaTipoTransaccion.NumVenta == numVenta
                    && v.ventaTipoTransaccion.CodComprobante == codComprobante
                    && v.ventaTipoTransaccion.CodModulo == codModulo
                    && v.ventaTipoTransaccion.NumSucursal == numSucursal
                    )
                    .Select(v => v.tipoTransaccion.Nombre)
                    .ToListAsync();

            try
            {
                VentasController ventasController = _ventasControllerFactory.Create();

                ventasController.ImprimirReporte(vMVentas_E, vMVentas_D, vMVentas_I, listaVentaTipoTransaccion);

                return Json(new { success = true, message = "Se realizó la reimpresión! :)" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al realizar la reimpresión :(" });
            }
        }

        // GET: Ventas_E/Create
        public IActionResult Create()
        {
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente");
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante");
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal");
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario");
            return View();
        }

        // POST: Ventas_E/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Fecha,Hora,id_FormaPago,CodCliente,id_Usuario,NumCaja")] VMVentas_E vMVentas_E)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMVentas_E);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // GET: Ventas_E/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMVentas_E == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E.FindAsync(id);
            if (vMVentas_E == null)
            {
                return NotFound();
            }
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // POST: Ventas_E/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Fecha,Hora,id_FormaPago,CodCliente,id_Usuario,NumCaja")] VMVentas_E vMVentas_E)
        {
            if (id != vMVentas_E.NumVenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMVentas_E);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMVentas_EExists(vMVentas_E.NumVenta))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // GET: Ventas_E/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMVentas_E == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_E == null)
            {
                return NotFound();
            }

            return View(vMVentas_E);
        }

        // POST: Ventas_E/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMVentas_E == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMVentas_E'  is null.");
            }
            var vMVentas_E = await _context.VMVentas_E.FindAsync(id);
            if (vMVentas_E != null)
            {
                _context.VMVentas_E.Remove(vMVentas_E);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMVentas_EExists(string id)
        {
          return _context.VMVentas_E.Any(e => e.NumVenta == id);
        }

        [HttpPost]
        public async Task<IActionResult> FiltrarVentasEntreFechas(string fechaInicio, string fechaFin)
        {
            if (fechaInicio == null || fechaFin == null)
            {
                TempData["ErrorMessage"] = "Las fechas de inicio y fin son requeridas.";
                return RedirectToAction("Index");
            }

            DateTime fechaInicioDate = DateTime.Parse(fechaInicio);
            DateTime fechaFinDate = DateTime.Parse(fechaFin);

            var ventas = await _context.VMVentas_E
                .Where(ve => ve.Fecha >= fechaInicioDate && ve.Fecha <= fechaFinDate)
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .Include(v => v.Ventas_D)
                .Include(v => v.Ventas_I)
                .OrderByDescending(v => v.Fecha)
                .ThenByDescending(v => v.Hora)
                .ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            HttpContext.Session.SetString("VentasFiltradas", JsonConvert.SerializeObject(ventas, settings));

            return RedirectToAction("Index", "Ventas_E");
        }
    }
}
