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
    public class Ventas_IController : Controller
    {
        private readonly MVCVentasContext _context;

        public Ventas_IController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Ventas_I
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMVentas_I.Include(v => v.Comprobante).Include(v => v.Concepto).Include(v => v.Modulo).Include(v => v.Sucursal).Include(v => v.Ventas_E);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Ventas_I/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMVentas_I == null)
            {
                return NotFound();
            }

            var vMVentas_I = await _context.VMVentas_I
                .Include(v => v.Comprobante)
                .Include(v => v.Concepto)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Ventas_E)
                .FirstOrDefaultAsync(m => m.NumVenta == id);


            if (vMVentas_I == null)
            {
                return NotFound();
            }

            return View();
        }

        // GET: Ventas_I/Create
        public IActionResult Create()
        {
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante");
            ViewData["CodConcepto"] = new SelectList(_context.VMConcepto, "CodConcepto", "CodConcepto");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal");
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta");
            return View();
        }

        // POST: Ventas_I/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,CodConcepto,Importe,Descuento")] VMVentas_I vMVentas_I)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMVentas_I);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_I.CodComprobante);
            ViewData["CodConcepto"] = new SelectList(_context.VMConcepto, "CodConcepto", "CodConcepto", vMVentas_I.CodConcepto);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_I.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_I.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_I.NumVenta);
            return View(vMVentas_I);
        }

        // GET: Ventas_I/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMVentas_I == null)
            {
                return NotFound();
            }

            var vMVentas_I = await _context.VMVentas_I.FindAsync(id);
            if (vMVentas_I == null)
            {
                return NotFound();
            }
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_I.CodComprobante);
            ViewData["CodConcepto"] = new SelectList(_context.VMConcepto, "CodConcepto", "CodConcepto", vMVentas_I.CodConcepto);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_I.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_I.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_I.NumVenta);
            return View(vMVentas_I);
        }

        // POST: Ventas_I/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,CodConcepto,Importe,Descuento")] VMVentas_I vMVentas_I)
        {
            if (id != vMVentas_I.NumVenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMVentas_I);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMVentas_IExists(vMVentas_I.NumVenta))
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
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_I.CodComprobante);
            ViewData["CodConcepto"] = new SelectList(_context.VMConcepto, "CodConcepto", "CodConcepto", vMVentas_I.CodConcepto);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_I.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_I.NumSucursal);
            ViewData["NumVenta"] = new SelectList(_context.VMVentas_E, "NumVenta", "NumVenta", vMVentas_I.NumVenta);
            return View(vMVentas_I);
        }

        // GET: Ventas_I/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMVentas_I == null)
            {
                return NotFound();
            }

            var vMVentas_I = await _context.VMVentas_I
                .Include(v => v.Comprobante)
                .Include(v => v.Concepto)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Ventas_E)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_I == null)
            {
                return NotFound();
            }

            return View(vMVentas_I);
        }

        // POST: Ventas_I/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMVentas_I == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMVentas_I'  is null.");
            }
            var vMVentas_I = await _context.VMVentas_I.FindAsync(id);
            if (vMVentas_I != null)
            {
                _context.VMVentas_I.Remove(vMVentas_I);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMVentas_IExists(string id)
        {
          return _context.VMVentas_I.Any(e => e.NumVenta == id);
        }
    }
}
