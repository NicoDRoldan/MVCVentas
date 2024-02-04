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
    public class PromoDescuento_DController : Controller
    {
        private readonly MVCVentasContext _context;

        public PromoDescuento_DController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: PromoDescuento_D
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMPromoDescuento_D
                  .Include(p => p.PromoDescuento_E)
                  .Include(p => p.Articulo)
                  .ToListAsync());
        }

        // GET: PromoDescuento_D/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMPromoDescuento_D == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_D = await _context.VMPromoDescuento_D
                .FirstOrDefaultAsync(m => m.Id_Promocion == id);
            if (vMPromoDescuento_D == null)
            {
                return NotFound();
            }

            return View(vMPromoDescuento_D);
        }

        // GET: PromoDescuento_D/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PromoDescuento_D/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Promocion,Id_Articulo")] VMPromoDescuento_D vMPromoDescuento_D)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMPromoDescuento_D);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMPromoDescuento_D);
        }

        // GET: PromoDescuento_D/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMPromoDescuento_D == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_D = await _context.VMPromoDescuento_D.FindAsync(id);
            if (vMPromoDescuento_D == null)
            {
                return NotFound();
            }
            return View(vMPromoDescuento_D);
        }

        // POST: PromoDescuento_D/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Promocion,Id_Articulo")] VMPromoDescuento_D vMPromoDescuento_D)
        {
            if (id != vMPromoDescuento_D.Id_Promocion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMPromoDescuento_D);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMPromoDescuento_DExists(vMPromoDescuento_D.Id_Promocion))
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
            return View(vMPromoDescuento_D);
        }

        // GET: PromoDescuento_D/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMPromoDescuento_D == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_D = await _context.VMPromoDescuento_D
                .FirstOrDefaultAsync(m => m.Id_Promocion == id);
            if (vMPromoDescuento_D == null)
            {
                return NotFound();
            }

            return View(vMPromoDescuento_D);
        }

        // POST: PromoDescuento_D/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMPromoDescuento_D == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMPromoDescuento_D'  is null.");
            }
            var vMPromoDescuento_D = await _context.VMPromoDescuento_D.FindAsync(id);
            if (vMPromoDescuento_D != null)
            {
                _context.VMPromoDescuento_D.Remove(vMPromoDescuento_D);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMPromoDescuento_DExists(int id)
        {
          return _context.VMPromoDescuento_D.Any(e => e.Id_Promocion == id);
        }
    }
}
