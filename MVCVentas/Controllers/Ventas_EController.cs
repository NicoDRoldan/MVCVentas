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
    public class Ventas_EController : Controller
    {
        private readonly MVCVentasContext _context;

        public Ventas_EController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Ventas_E
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMVentas_E
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Ventas_E/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMVentas_E == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_E == null)
            {
                return NotFound();
            }

            return View(vMVentas_E);
        }

        // GET: Ventas_E/Create
        public IActionResult Create()
        {
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente");
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante");
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal");
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario");
            return View();
        }

        // POST: Ventas_E/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Fecha,Hora,id_FormaPago,CodCliente,id_Usuario,NumCaja")] VMVentas_E vMVentas_E)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMVentas_E);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // GET: Ventas_E/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMVentas_E == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E.FindAsync(id);
            if (vMVentas_E == null)
            {
                return NotFound();
            }
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // POST: Ventas_E/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NumVenta,CodComprobante,CodModulo,NumSucursal,Fecha,Hora,id_FormaPago,CodCliente,id_Usuario,NumCaja")] VMVentas_E vMVentas_E)
        {
            if (id != vMVentas_E.NumVenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMVentas_E);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMVentas_EExists(vMVentas_E.NumVenta))
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
            ViewData["CodCliente"] = new SelectList(_context.VMCliente, "CodCliente", "CodCliente", vMVentas_E.CodCliente);
            ViewData["CodComprobante"] = new SelectList(_context.VMComprobante_E, "CodComprobante", "CodComprobante", vMVentas_E.CodComprobante);
            ViewData["id_FormaPago"] = new SelectList(_context.VMFormaPago, "Id_FormaPago", "Id_FormaPago", vMVentas_E.id_FormaPago);
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMVentas_E.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMVentas_E.NumSucursal);
            ViewData["id_Usuario"] = new SelectList(_context.VMUser, "Id_Usuario", "Usuario", vMVentas_E.id_Usuario);
            return View(vMVentas_E);
        }

        // GET: Ventas_E/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMVentas_E == null)
            {
                return NotFound();
            }

            var vMVentas_E = await _context.VMVentas_E
                .Include(v => v.Cliente)
                .Include(v => v.Comprobante)
                .Include(v => v.FormaPago)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(m => m.NumVenta == id);
            if (vMVentas_E == null)
            {
                return NotFound();
            }

            return View(vMVentas_E);
        }

        // POST: Ventas_E/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMVentas_E == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMVentas_E'  is null.");
            }
            var vMVentas_E = await _context.VMVentas_E.FindAsync(id);
            if (vMVentas_E != null)
            {
                _context.VMVentas_E.Remove(vMVentas_E);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMVentas_EExists(string id)
        {
          return _context.VMVentas_E.Any(e => e.NumVenta == id);
        }
    }
}
