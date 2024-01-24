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
    public class PricesController : Controller
    {
        private readonly MVCVentasContext _context;

        public PricesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Prices
        public async Task<IActionResult> Index()
        {
              return _context.VMPrice != null ? 
                          View(await _context.VMPrice
                          .Include(p => p.Articulo)
                          .ToListAsync()) :
                          Problem("Entity set 'MVCVentasContext.VMPrice'  is null.");
        }

        // GET: Prices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMPrice == null)
            {
                return NotFound();
            }

            var vMPrice = await _context.VMPrice
                .Include(p => p.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMPrice == null)
            {
                return NotFound();
            }

            return View(vMPrice);
        }

        // GET: Prices/Create
        public IActionResult Create()
        {
            ViewBag.Articulos = new SelectList(_context.VMArticle.ToList(), "Id_Articulo", "Nombre");

            return View();
        }

        // POST: Prices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Articulo,Precio,Fecha")] VMPrice vMPrice)
        {
            vMPrice.Fecha = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(vMPrice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Articulos = new SelectList(_context.VMArticle.ToList(), "Id_Articulo", "Nombre");

            return View(vMPrice);
        }

        // GET: Prices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMPrice == null)
            {
                return NotFound();
            }

            var vMPrice = await _context.VMPrice
                .Include(u => u.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);

            if (vMPrice == null)
            {
                return NotFound();
            }

            ViewBag.Articulos = new SelectList(_context.VMArticle.ToList(), "Id_Articulo", "Nombre");

            return View(vMPrice);
        }

        // POST: Prices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Articulo,Precio,Fecha")] VMPrice vMPrice)
        {
            if (id != vMPrice.Id_Articulo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMPrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMPriceExists(vMPrice.Id_Articulo))
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
            return View(vMPrice);
        }

        // GET: Prices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMPrice == null)
            {
                return NotFound();
            }

            var vMPrice = await _context.VMPrice
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMPrice == null)
            {
                return NotFound();
            }

            return View(vMPrice);
        }

        // POST: Prices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMPrice == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMPrice'  is null.");
            }
            var vMPrice = await _context.VMPrice.FindAsync(id);
            if (vMPrice != null)
            {
                _context.VMPrice.Remove(vMPrice);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMPriceExists(int id)
        {
          return (_context.VMPrice?.Any(e => e.Id_Articulo == id)).GetValueOrDefault();
        }
    }
}
