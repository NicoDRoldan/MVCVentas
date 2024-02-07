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
    public class ModulosController : Controller
    {
        private readonly MVCVentasContext _context;

        public ModulosController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Modulos
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMModulo.ToListAsync());
        }

        // GET: Modulos/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMModulo == null)
            {
                return NotFound();
            }

            var vMModulo = await _context.VMModulo
                .FirstOrDefaultAsync(m => m.CodModulo == id);
            if (vMModulo == null)
            {
                return NotFound();
            }

            return View(vMModulo);
        }

        // GET: Modulos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Modulos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodModulo,Descripcion")] VMModulo vMModulo)
        {
            var ModuloExiste = await _context.VMModulo
                                    .FirstOrDefaultAsync(m => m.CodModulo == vMModulo.CodModulo);

            if(ModuloExiste != null)        
            {
                TempData["MensajeError"] = Uri.EscapeDataString("El código del módulo indicado ya existe.");
                return RedirectToAction("Create");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vMModulo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMModulo);
        }

        // GET: Modulos/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMModulo == null)
            {
                return NotFound();
            }

            var vMModulo = await _context.VMModulo.FindAsync(id);
            if (vMModulo == null)
            {
                return NotFound();
            }
            return View(vMModulo);
        }

        // POST: Modulos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodModulo,Descripcion")] VMModulo vMModulo)
        {
            if (id != vMModulo.CodModulo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMModulo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMModuloExists(vMModulo.CodModulo))
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
            return View(vMModulo);
        }

        // GET: Modulos/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMModulo == null)
            {
                return NotFound();
            }

            var vMModulo = await _context.VMModulo
                .FirstOrDefaultAsync(m => m.CodModulo == id);
            if (vMModulo == null)
            {
                return NotFound();
            }

            return View(vMModulo);
        }

        // POST: Modulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMModulo == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMModulo'  is null.");
            }
            
            var vMModulo = await _context.VMModulo.FindAsync(id);
            var ModuloExiste = await _context.VMModulo
                                    .Include(m => m.Comprobantes_N)
                                    .FirstOrDefaultAsync(m => m.CodModulo == id);

            if(ModuloExiste != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("No se puede eliminar el módulo, ya que tiene comprobantes asociados.");
                return RedirectToAction("Delete");
            }

            if (vMModulo != null)
            {
                _context.VMModulo.Remove(vMModulo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMModuloExists(string id)
        {
          return _context.VMModulo.Any(e => e.CodModulo == id);
        }
    }
}
