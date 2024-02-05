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
    public class Comprobantes_EController : Controller
    {
        private readonly MVCVentasContext _context;

        public Comprobantes_EController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Comprobantes_E
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMComprobante_E.Include(v => v.Modulo);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Comprobantes_E/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMComprobante_E == null)
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E
                .Include(v => v.Modulo)
                .FirstOrDefaultAsync(m => m.CodComprobante == id);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }

            return View(vMComprobante_E);
        }

        // GET: Comprobantes_E/Create
        public IActionResult Create()
        {
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            return View();
        }

        // POST: Comprobantes_E/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodComprobante,Nombre,CodModulo")] VMComprobante_E vMComprobante_E)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMComprobante_E);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        // GET: Comprobantes_E/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMComprobante_E == null)
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E.FindAsync(id);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        // POST: Comprobantes_E/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodComprobante,Nombre,CodModulo")] VMComprobante_E vMComprobante_E)
        {
            if (id != vMComprobante_E.CodComprobante)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMComprobante_E);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMComprobante_EExists(vMComprobante_E.CodComprobante))
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
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        // GET: Comprobantes_E/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMComprobante_E == null)
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E
                .Include(v => v.Modulo)
                .FirstOrDefaultAsync(m => m.CodComprobante == id);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }

            return View(vMComprobante_E);
        }

        // POST: Comprobantes_E/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMComprobante_E == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMComprobante_E'  is null.");
            }
            var vMComprobante_E = await _context.VMComprobante_E.FindAsync(id);
            if (vMComprobante_E != null)
            {
                _context.VMComprobante_E.Remove(vMComprobante_E);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMComprobante_EExists(string id)
        {
          return _context.VMComprobante_E.Any(e => e.CodComprobante == id);
        }
    }
}
