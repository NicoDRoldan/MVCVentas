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
    public class SucursalesController : Controller
    {
        private readonly MVCVentasContext _context;

        public SucursalesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Sucursales
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMSucursal.Include(v => v.TipoFactura);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Sucursales/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMSucursal == null)
            {
                return NotFound();
            }

            var vMSucursal = await _context.VMSucursal
                .Include(v => v.TipoFactura)
                .FirstOrDefaultAsync(m => m.NumSucursal == id);
            if (vMSucursal == null)
            {
                return NotFound();
            }

            return View(vMSucursal);
        }

        // GET: Sucursales/Create
        public IActionResult Create()
        {
            ViewData["Id_TipoFactura"] = new SelectList(_context.Set<VMTipoFactura>(), "Id_TipoFactura", "Nombre");
            return View();
        }

        // POST: Sucursales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumSucursal,Id_TipoFactura")] VMSucursal vMSucursal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMSucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id_TipoFactura"] = new SelectList(_context.Set<VMTipoFactura>(), "Id_TipoFactura", "Id_TipoFactura", vMSucursal.Id_TipoFactura);
            return View(vMSucursal);
        }

        // GET: Sucursales/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMSucursal == null)
            {
                return NotFound();
            }

            var vMSucursal = await _context.VMSucursal.FindAsync(id);
            if (vMSucursal == null)
            {
                return NotFound();
            }
            ViewData["Id_TipoFactura"] = new SelectList(_context.Set<VMTipoFactura>(), "Id_TipoFactura", "Nombre", vMSucursal.Id_TipoFactura);
            return View(vMSucursal);
        }

        // POST: Sucursales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NumSucursal,Id_TipoFactura")] VMSucursal vMSucursal)
        {
            if (id != vMSucursal.NumSucursal)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMSucursal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMSucursalExists(vMSucursal.NumSucursal))
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
            ViewData["Id_TipoFactura"] = new SelectList(_context.Set<VMTipoFactura>(), "Id_TipoFactura", "Id_TipoFactura", vMSucursal.Id_TipoFactura);
            return View(vMSucursal);
        }

        // GET: Sucursales/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMSucursal == null)
            {
                return NotFound();
            }

            var vMSucursal = await _context.VMSucursal
                .Include(v => v.TipoFactura)
                .FirstOrDefaultAsync(m => m.NumSucursal == id);
            if (vMSucursal == null)
            {
                return NotFound();
            }

            return View(vMSucursal);
        }

        // POST: Sucursales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMSucursal == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMSucursal'  is null.");
            }
            var vMSucursal = await _context.VMSucursal.FindAsync(id);
            if (vMSucursal != null)
            {
                _context.VMSucursal.Remove(vMSucursal);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMSucursalExists(string id)
        {
          return _context.VMSucursal.Any(e => e.NumSucursal == id);
        }
    }
}
