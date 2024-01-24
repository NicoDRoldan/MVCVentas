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
    public class ArticlesController : Controller
    {
        private readonly MVCVentasContext _context;

        public ArticlesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
              return _context.VMArticle != null ? 
                          View(await _context.VMArticle.ToListAsync()) :
                          Problem("Entity set 'MVCVentasContext.VMArticle'  is null.");
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMArticle = await _context.VMArticle
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMArticle == null)
            {
                return NotFound();
            }

            return View(vMArticle);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Articulo,Nombre,Rubro,Activo,Descripcion,Fecha")] VMArticle vMArticle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMArticle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMArticle);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMArticle = await _context.VMArticle.FindAsync(id);
            if (vMArticle == null)
            {
                return NotFound();
            }
            return View(vMArticle);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Articulo,Nombre,Rubro,Activo,Descripcion,Fecha")] VMArticle vMArticle)
        {
            if (id != vMArticle.Id_Articulo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMArticle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMArticleExists(vMArticle.Id_Articulo))
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
            return View(vMArticle);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMArticle = await _context.VMArticle
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMArticle == null)
            {
                return NotFound();
            }

            return View(vMArticle);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMArticle == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMArticle'  is null.");
            }
            var vMArticle = await _context.VMArticle.FindAsync(id);
            if (vMArticle != null)
            {
                _context.VMArticle.Remove(vMArticle);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMArticleExists(int id)
        {
          return (_context.VMArticle?.Any(e => e.Id_Articulo == id)).GetValueOrDefault();
        }
    }
}
