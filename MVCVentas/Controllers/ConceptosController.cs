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
    public class ConceptosController : Controller
    {
        private readonly MVCVentasContext _context;

        public ConceptosController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Conceptos
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMConcepto.ToListAsync());
        }

        // GET: Conceptos/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMConcepto == null)
            {
                return NotFound();
            }

            var vMConcepto = await _context.VMConcepto
                .FirstOrDefaultAsync(m => m.CodConcepto == id);
            if (vMConcepto == null)
            {
                return NotFound();
            }

            return View(vMConcepto);
        }

        // GET: Conceptos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Conceptos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodConcepto,Descripcion,Porcentaje")] VMConcepto vMConcepto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMConcepto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMConcepto);
        }

        // GET: Conceptos/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMConcepto == null)
            {
                return NotFound();
            }

            var vMConcepto = await _context.VMConcepto.FindAsync(id);
            if (vMConcepto == null)
            {
                return NotFound();
            }
            return View(vMConcepto);
        }

        // POST: Conceptos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodConcepto,Descripcion,Porcentaje")] VMConcepto vMConcepto)
        {
            if (id != vMConcepto.CodConcepto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMConcepto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMConceptoExists(vMConcepto.CodConcepto))
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
            return View(vMConcepto);
        }

        // GET: Conceptos/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMConcepto == null)
            {
                return NotFound();
            }

            var vMConcepto = await _context.VMConcepto
                .FirstOrDefaultAsync(m => m.CodConcepto == id);
            if (vMConcepto == null)
            {
                return NotFound();
            }

            return View(vMConcepto);
        }

        // POST: Conceptos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMConcepto == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMConcepto'  is null.");
            }
            var vMConcepto = await _context.VMConcepto.FindAsync(id);
            if (vMConcepto != null)
            {
                _context.VMConcepto.Remove(vMConcepto);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMConceptoExists(string id)
        {
          return _context.VMConcepto.Any(e => e.CodConcepto == id);
        }
    }
}
