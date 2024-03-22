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
    public class CombosController : Controller
    {
        private readonly MVCVentasContext _context;

        public CombosController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Combos
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = await _context.VMCombo
                .Include(v => v.Articulo)
                .GroupBy(c => c.Articulo)
                .Select(g => g.Key)
                .ToListAsync();

            return View(mVCVentasContext);
        }

        // GET: Combos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMCombo = await (from c in _context.VMCombo
                                 join a in _context.VMArticle on c.Id_ArticuloAgregado equals a.Id_Articulo
                                 where c.Id_Articulo == id
                                 select new Tuple<int, string>(c.Id_ArticuloAgregado, a.Nombre)
                                  ).ToListAsync();

            return View(vMCombo);
        }

        // GET: Combos/Create
        public IActionResult Create()
        {
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo");
            return View();
        }

        // POST: Combos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Combo,Id_Articulo,Id_ArticuloAgregado")] VMCombo vMCombo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMCombo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMCombo.Id_Articulo);
            return View(vMCombo);
        }

        // GET: Combos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMCombo == null)
            {
                return NotFound();
            }

            var vMCombo = await _context.VMCombo.FindAsync(id);
            if (vMCombo == null)
            {
                return NotFound();
            }
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMCombo.Id_Articulo);
            return View(vMCombo);
        }

        // POST: Combos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Combo,Id_Articulo,Id_ArticuloAgregado")] VMCombo vMCombo)
        {
            if (id != vMCombo.Id_Combo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMCombo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMComboExists(vMCombo.Id_Combo))
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
            ViewData["Id_Articulo"] = new SelectList(_context.VMArticle, "Id_Articulo", "Id_Articulo", vMCombo.Id_Articulo);
            return View(vMCombo);
        }

        // GET: Combos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMCombo == null)
            {
                return NotFound();
            }

            var vMCombo = await _context.VMCombo
                .Include(v => v.Articulo)
                .FirstOrDefaultAsync(m => m.Id_Combo == id);
            if (vMCombo == null)
            {
                return NotFound();
            }

            return View(vMCombo);
        }

        // POST: Combos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMCombo == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMCombo'  is null.");
            }
            var vMCombo = await _context.VMCombo.FindAsync(id);
            if (vMCombo != null)
            {
                _context.VMCombo.Remove(vMCombo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMComboExists(int id)
        {
          return _context.VMCombo.Any(e => e.Id_Combo == id);
        }
    }
}
