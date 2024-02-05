using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    public class Comprobantes_EController : Controller
    {
        private readonly MVCVentasContext _context;

        public Comprobantes_EController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Comprobantes_E
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMComprobante_E.Include(v => v.Modulo);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Comprobantes_E/Details/5
        public async Task<IActionResult> Details (string codComprobante, string codModulo)
        {
            if ((codComprobante == null && codModulo == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E
                .Include(v => v.Modulo)
                .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }

            return View(vMComprobante_E);
        }

        // GET: Comprobantes_E/Create
        public IActionResult Create()
        {
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            return View();
        }

        // POST: Comprobantes_E/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodComprobante,Nombre,CodModulo")] VMComprobante_E vMComprobante_E)
        {
            var CodComprobante = await _context.VMComprobante_E
                .FirstOrDefaultAsync(m => m.CodComprobante == vMComprobante_E.CodComprobante
                                            && m.CodModulo == vMComprobante_E.CodModulo);

            if(CodComprobante != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("Error, ya existe un Código de Comprobante para el Módulo indicado.");
                return RedirectToAction("Create");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vMComprobante_E);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        // GET: Comprobantes_E/Edit/5
        public async Task<IActionResult> Edit(string codComprobante, string codModulo)
        {
            if ((codComprobante == null && codModulo == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E
                .Include(v => v.Modulo)
                .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string codComprobante, string codModulo, [Bind("CodComprobante,Nombre,CodModulo")] VMComprobante_E vMComprobante_E)
        {
            if (codComprobante != vMComprobante_E.CodComprobante)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMComprobante_E);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMComprobante_EExists(vMComprobante_E.CodComprobante, vMComprobante_E.CodModulo))
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
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_E.CodModulo);
            return View(vMComprobante_E);
        }

        public async Task<IActionResult> Delete(string codComprobante, string codModulo)
        {
            if ((codComprobante == null && codModulo == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_E = await _context.VMComprobante_E
                .Include(v => v.Modulo)
                .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo);
            if (vMComprobante_E == null)
            {
                return NotFound();
            }

            return View(vMComprobante_E);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string codComprobante, string codModulo)
        {
            if (_context.VMComprobante_E == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMComprobante_E'  is null.");
            }
            var vMComprobante_E = await _context.VMComprobante_E.FindAsync(codComprobante, codModulo);
            if (vMComprobante_E != null)
            {
                _context.VMComprobante_E.Remove(vMComprobante_E);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMComprobante_EExists(string codComprobante, string codModulo)
        {
          return _context.VMComprobante_E.Any(e => e.CodComprobante == codComprobante && e.CodModulo == codModulo);
        }
    }
}
