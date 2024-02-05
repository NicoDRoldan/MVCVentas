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
    public class ClientesController : Controller
    {
        private readonly MVCVentasContext _context;

        public ClientesController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMCliente.Include(v => v.Localidad).Include(v => v.Provincia);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMCliente == null)
            {
                return NotFound();
            }

            var vMCliente = await _context.VMCliente
                .Include(v => v.Localidad)
                .Include(v => v.Provincia)
                .FirstOrDefaultAsync(m => m.CodCliente == id);
            if (vMCliente == null)
            {
                return NotFound();
            }

            return View(vMCliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "Nombre");
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "Nombre");
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodCliente,CUIT,RazonSocial,Nombre,Telefono,Email,Direccion,CodProvincia,CodLocalidad,FechaAlta")] VMCliente vMCliente)
        {
            if (ModelState.IsValid)
            {
                vMCliente.FechaAlta = DateTime.Now;
                vMCliente.CodCliente = vMCliente.CUIT;

                _context.Add(vMCliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "CodLocalidad", vMCliente.CodLocalidad);
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "CodProvincia", vMCliente.CodProvincia);
            return View(vMCliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMCliente == null)
            {
                return NotFound();
            }

            var vMCliente = await _context.VMCliente.FindAsync(id);
            if (vMCliente == null)
            {
                return NotFound();
            }
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "Nombre", vMCliente.CodLocalidad);
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "Nombre", vMCliente.CodProvincia);
            return View(vMCliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodCliente,CUIT,RazonSocial,Nombre,Telefono,Email,Direccion,CodProvincia,CodLocalidad,FechaAlta")] VMCliente vMCliente)
        {
            if (id != vMCliente.CodCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMCliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMClienteExists(vMCliente.CodCliente))
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
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "CodLocalidad", vMCliente.CodLocalidad);
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "CodProvincia", vMCliente.CodProvincia);
            return View(vMCliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMCliente == null)
            {
                return NotFound();
            }

            var vMCliente = await _context.VMCliente
                .Include(v => v.Localidad)
                .Include(v => v.Provincia)
                .FirstOrDefaultAsync(m => m.CodCliente == id);
            if (vMCliente == null)
            {
                return NotFound();
            }

            return View(vMCliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMCliente == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMCliente'  is null.");
            }
            var vMCliente = await _context.VMCliente.FindAsync(id);
            if (vMCliente != null)
            {
                _context.VMCliente.Remove(vMCliente);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMClienteExists(string id)
        {
          return _context.VMCliente.Any(e => e.CodCliente == id);
        }
    }
}
