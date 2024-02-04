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
    public class TipoPromoDescuentoesController : Controller
    {
        private readonly MVCVentasContext _context;

        public TipoPromoDescuentoesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: TipoPromoDescuentoes
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMTipoPromoDescuento.ToListAsync());
        }

        // GET: TipoPromoDescuentoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMTipoPromoDescuento == null)
            {
                return NotFound();
            }

            var vMTipoPromoDescuento = await _context.VMTipoPromoDescuento
                .FirstOrDefaultAsync(m => m.Id_Tipo == id);
            if (vMTipoPromoDescuento == null)
            {
                return NotFound();
            }

            return View(vMTipoPromoDescuento);
        }

        // GET: TipoPromoDescuentoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoPromoDescuentoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Tipo,Descripcion")] VMTipoPromoDescuento vMTipoPromoDescuento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMTipoPromoDescuento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMTipoPromoDescuento);
        }

        // GET: TipoPromoDescuentoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMTipoPromoDescuento == null)
            {
                return NotFound();
            }

            var vMTipoPromoDescuento = await _context.VMTipoPromoDescuento.FindAsync(id);
            if (vMTipoPromoDescuento == null)
            {
                return NotFound();
            }
            return View(vMTipoPromoDescuento);
        }

        // POST: TipoPromoDescuentoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Tipo,Descripcion")] VMTipoPromoDescuento vMTipoPromoDescuento)
        {
            if (id != vMTipoPromoDescuento.Id_Tipo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMTipoPromoDescuento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMTipoPromoDescuentoExists(vMTipoPromoDescuento.Id_Tipo))
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
            return View(vMTipoPromoDescuento);
        }

        // GET: TipoPromoDescuentoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMTipoPromoDescuento == null)
            {
                return NotFound();
            }

            var vMTipoPromoDescuento = await _context.VMTipoPromoDescuento
                .FirstOrDefaultAsync(m => m.Id_Tipo == id);
            if (vMTipoPromoDescuento == null)
            {
                return NotFound();
            }

            return View(vMTipoPromoDescuento);
        }

        // POST: TipoPromoDescuentoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMTipoPromoDescuento == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMTipoPromoDescuento'  is null.");
            }

            var vMTipoPromoDescuento = await _context.VMTipoPromoDescuento.FindAsync(id);
            var tiposPromos = await _context.VMTipoPromoDescuento
                .Include(p => p.PromosDescuentosE)
                .FirstOrDefaultAsync(t => t.Id_Tipo == id);

            if (tiposPromos.PromosDescuentosE != null && tiposPromos.PromosDescuentosE.Any())
            {
                TempData["MensajeError"] = Uri.EscapeDataString("El tipo de promoción tiene una o más promociones. Debe eliminarlas para poder eliminar el tipo de promoción.");
                return RedirectToAction("Delete");
            }

            if (vMTipoPromoDescuento != null)
            {
                _context.VMTipoPromoDescuento.Remove(vMTipoPromoDescuento);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMTipoPromoDescuentoExists(int id)
        {
          return _context.VMTipoPromoDescuento.Any(e => e.Id_Tipo == id);
        }
    }
}
