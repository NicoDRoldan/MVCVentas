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
    public class CategoriesController : Controller
    {
        private readonly MVCVentasContext _context;

        public CategoriesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
              return _context.VMCategory != null ? 
                          View(await _context.VMCategory.ToListAsync()) :
                          Problem("Entity set 'MVCVentasContext.VMCategory'  is null.");
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMCategory == null)
            {
                return NotFound();
            }

            var vMCategory = await _context.VMCategory
                .FirstOrDefaultAsync(m => m.Id_Categoria == id);
            if (vMCategory == null)
            {
                return NotFound();
            }

            return View(vMCategory);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Categoria,Nombre")] VMCategory vMCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMCategory);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMCategory == null)
            {
                return NotFound();
            }

            var vMCategory = await _context.VMCategory.FindAsync(id);
            if (vMCategory == null)
            {
                return NotFound();
            }
            return View(vMCategory);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Categoria,Nombre")] VMCategory vMCategory)
        {
            if (id != vMCategory.Id_Categoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMCategoryExists(vMCategory.Id_Categoria))
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
            return View(vMCategory);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMCategory == null)
            {
                return NotFound();
            }

            var vMCategory = await _context.VMCategory
                .FirstOrDefaultAsync(m => m.Id_Categoria == id);
            if (vMCategory == null)
            {
                return NotFound();
            }

            return View(vMCategory);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMCategory == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMCategory'  is null.");
            }
            var vMCategory = await _context.VMCategory.FindAsync(id);
            if (vMCategory != null)
            {
                _context.VMCategory.Remove(vMCategory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMCategoryExists(int id)
        {
          return (_context.VMCategory?.Any(e => e.Id_Categoria == id)).GetValueOrDefault();
        }
    }
}
