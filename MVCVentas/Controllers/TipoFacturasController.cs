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
    public class TipoFacturasController : Controller
    {
        private readonly MVCVentasContext _context;

        public TipoFacturasController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: TipoFacturas
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMTipoFactura.ToListAsync());
        }

        // GET: TipoFacturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMTipoFactura == null)
            {
                return NotFound();
            }

            var vMTipoFactura = await _context.VMTipoFactura
                .FirstOrDefaultAsync(m => m.Id_TipoFactura == id);
            if (vMTipoFactura == null)
            {
                return NotFound();
            }

            return View(vMTipoFactura);
        }

        // GET: TipoFacturas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoFacturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_TipoFactura,Nombre")] VMTipoFactura vMTipoFactura)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMTipoFactura);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMTipoFactura);
        }

        // GET: TipoFacturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMTipoFactura == null)
            {
                return NotFound();
            }

            var vMTipoFactura = await _context.VMTipoFactura.FindAsync(id);
            if (vMTipoFactura == null)
            {
                return NotFound();
            }
            return View(vMTipoFactura);
        }

        // POST: TipoFacturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_TipoFactura,Nombre")] VMTipoFactura vMTipoFactura)
        {
            if (id != vMTipoFactura.Id_TipoFactura)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMTipoFactura);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMTipoFacturaExists(vMTipoFactura.Id_TipoFactura))
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
            return View(vMTipoFactura);
        }

        // GET: TipoFacturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMTipoFactura == null)
            {
                return NotFound();
            }

            var vMTipoFactura = await _context.VMTipoFactura
                .FirstOrDefaultAsync(m => m.Id_TipoFactura == id);
            if (vMTipoFactura == null)
            {
                return NotFound();
            }

            return View(vMTipoFactura);
        }

        // POST: TipoFacturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMTipoFactura == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMTipoFactura'  is null.");
            }
            var vMTipoFactura = await _context.VMTipoFactura.FindAsync(id);
            if (vMTipoFactura != null)
            {
                _context.VMTipoFactura.Remove(vMTipoFactura);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMTipoFacturaExists(int id)
        {
          return _context.VMTipoFactura.Any(e => e.Id_TipoFactura == id);
        }
    }
}
