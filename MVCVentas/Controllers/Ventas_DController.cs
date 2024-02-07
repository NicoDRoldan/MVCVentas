using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    public class Ventas_DController : Controller
    {
        private readonly MVCVentasContext _context;

        public Ventas_DController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Ventas_D
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMVentas_D.Include(v => v.Articulo).Include(v => v.Comprobante).Include(v => v.Modulo).Include(v => v.Sucursal).Include(v => v.Ventas_E);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Ventas_D/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMVentas_D == null)
            {
                return NotFound();
            }

            var vMVentas_D = await _context.VMVentas_D
                .Include(v => v.Articulo)
                .Include(v => v.Comprobante)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Ventas_E)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_D == null)
            {
                return NotFound();
            }

            return View(vMVentas_D);
        }

        // GET: Ventas_D/Create
        public IActionResult Create()
        {
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Nombre");
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal");
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta");
            return View();
        }

        // POST: Ventas_D/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Renglon,Id_Articulo,Cantidad,Detalle,PrecioUnitario,PrecioTotal")] VMVentas_D vMVentas_D)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMVentas_D);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Nombre", vMVentas_D.Id_Articulo);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_D.CodComprobante);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_D.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_D.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_D.NumVenta);
            return View(vMVentas_D);
        }

        // GET: Ventas_D/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMVentas_D == null)
            {
                return NotFound();
            }

            var vMVentas_D = await _context.VMVentas_D.FindAsync(id);
            if (vMVentas_D == null)
            {
                return NotFound();
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMVentas_D.Id_Articulo);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_D.CodComprobante);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_D.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_D.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_D.NumVenta);
            return View(vMVentas_D);
        }

        // POST: Ventas_D/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Renglon,Id_Articulo,Cantidad,Detalle,PrecioUnitario,PrecioTotal")] VMVentas_D vMVentas_D)
        {
            if (id != vMVentas_D.NumVenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMVentas_D);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMVentas_DExists(vMVentas_D.NumVenta))
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
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMVentas_D.Id_Articulo);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_D.CodComprobante);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_D.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_D.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_D.NumVenta);
            return View(vMVentas_D);
        }

        // GET: Ventas_D/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMVentas_D == null)
            {
                return NotFound();
            }

            var vMVentas_D = await _context.VMVentas_D
                .Include(v => v.Articulo)
                .Include(v => v.Comprobante)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Ventas_E)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_D == null)
            {
                return NotFound();
            }

            return View(vMVentas_D);
        }

        // POST: Ventas_D/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMVentas_D == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMVentas_D'  is null.");
            }
            var vMVentas_D = await _context.VMVentas_D.FindAsync(id);
            if (vMVentas_D != null)
            {
                _context.VMVentas_D.Remove(vMVentas_D);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMVentas_DExists(string id)
        {
          return _context.VMVentas_D.Any(e => e.NumVenta == id);
        }
    }
}
