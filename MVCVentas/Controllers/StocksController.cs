using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class StocksController : Controller
    {
        private readonly MVCVentasContext _context;

        public StocksController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Stocks
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMStock.Include(v => v.Articulo);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Stocks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMStock == null)
            {
                return NotFound();
            }

            var vMStock = await _context.VMStock
                .Include(v => v.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMStock == null)
            {
                return NotFound();
            }

            return View(vMStock);
        }

        // GET: Stocks/Create
        public IActionResult Create()
        {
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo");
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Articulo,Cantidad")] VMStock vMStock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMStock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMStock.Id_Articulo);
            return View(vMStock);
        }

        // GET: Stocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMStock == null)
            {
                return NotFound();
            }

            var vMStock = await _context.VMStock
                .Include(v => v.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);

            if (vMStock == null)
            {
                return NotFound();
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMStock.Id_Articulo);
            return View(vMStock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Articulo,Cantidad")] VMStock vMStock)
        {
            if (id != vMStock.Id_Articulo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMStock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMStockExists(vMStock.Id_Articulo))
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
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMStock.Id_Articulo);
            return View(vMStock);
        }

        // GET: Stocks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMStock == null)
            {
                return NotFound();
            }

            var vMStock = await _context.VMStock
                .Include(v => v.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMStock == null)
            {
                return NotFound();
            }

            return View(vMStock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMStock == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMStock'  is null.");
            }
            var vMStock = await _context.VMStock.FindAsync(id);
            if (vMStock != null)
            {
                _context.VMStock.Remove(vMStock);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMStockExists(int id)
        {
          return _context.VMStock.Any(e => e.Id_Articulo == id);
        }
    }
}
