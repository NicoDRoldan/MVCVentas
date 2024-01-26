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
    public class RubrosController : Controller
    {
        private readonly MVCVentasContext _context;

        public RubrosController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Rubros
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMRubro.ToListAsync());
        }

        // GET: Rubros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMRubro == null)
            {
                return NotFound();
            }

            var vMRubro = await _context.VMRubro
                .FirstOrDefaultAsync(m => m.Id_Rubro == id);
            if (vMRubro == null)
            {
                return NotFound();
            }

            return View(vMRubro);
        }

        // GET: Rubros/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rubros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Rubro,Nombre")] VMRubro vMRubro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMRubro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMRubro);
        }

        // GET: Rubros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMRubro == null)
            {
                return NotFound();
            }

            var vMRubro = await _context.VMRubro.FindAsync(id);
            if (vMRubro == null)
            {
                return NotFound();
            }
            return View(vMRubro);
        }

        // POST: Rubros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Rubro,Nombre")] VMRubro vMRubro)
        {
            if (id != vMRubro.Id_Rubro)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMRubro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMRubroExists(vMRubro.Id_Rubro))
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
            return View(vMRubro);
        }

        // GET: Rubros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMRubro == null)
            {
                return NotFound();
            }

            var vMRubro = await _context.VMRubro
                .FirstOrDefaultAsync(m => m.Id_Rubro == id);
            if (vMRubro == null)
            {
                return NotFound();
            }

            return View(vMRubro);
        }

        // POST: Rubros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMRubro == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMRubro'  is null.");
            }
            var vMRubro = await _context.VMRubro.FindAsync(id);
            if (vMRubro != null)
            {
                _context.VMRubro.Remove(vMRubro);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMRubroExists(int id)
        {
          return _context.VMRubro.Any(e => e.Id_Rubro == id);
        }
    }
}
