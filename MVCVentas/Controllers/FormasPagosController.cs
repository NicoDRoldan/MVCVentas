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
    public class FormasPagosController : Controller
    {
        private readonly MVCVentasContext _context;

        public FormasPagosController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: FormasPagos
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMFormaPago.ToListAsync());
        }

        // GET: FormasPagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMFormaPago == null)
            {
                return NotFound();
            }

            var vMFormaPago = await _context.VMFormaPago
                .FirstOrDefaultAsync(m => m.Id_FormaPago == id);
            if (vMFormaPago == null)
            {
                return NotFound();
            }

            return View(vMFormaPago);
        }

        // GET: FormasPagos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FormasPagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_FormaPago,Nombre")] VMFormaPago vMFormaPago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMFormaPago);

                string codTipoTran = vMFormaPago.Nombre.Replace(" ", "").ToUpper();

                var tipoTran = await _context.VMTipoTransaccion
                    .Where(t => t.CodTipoTran == codTipoTran)
                    .FirstOrDefaultAsync();

                if(tipoTran is null)
                {
                    var tipoTransaccion = new VMTipoTransaccion
                    {
                        CodTipoTran = codTipoTran,
                        Nombre = vMFormaPago.Nombre
                    };

                    _context.Add(tipoTransaccion);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(vMFormaPago);
        }

        // GET: FormasPagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMFormaPago == null)
            {
                return NotFound();
            }

            var vMFormaPago = await _context.VMFormaPago.FindAsync(id);
            if (vMFormaPago == null)
            {
                return NotFound();
            }
            return View(vMFormaPago);
        }

        // POST: FormasPagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_FormaPago,Nombre")] VMFormaPago vMFormaPago)
        {
            if (id != vMFormaPago.Id_FormaPago)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMFormaPago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMFormaPagoExists(vMFormaPago.Id_FormaPago))
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
            return View(vMFormaPago);
        }

        // GET: FormasPagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMFormaPago == null)
            {
                return NotFound();
            }

            var vMFormaPago = await _context.VMFormaPago
                .FirstOrDefaultAsync(m => m.Id_FormaPago == id);
            if (vMFormaPago == null)
            {
                return NotFound();
            }

            return View(vMFormaPago);
        }

        // POST: FormasPagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMFormaPago == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMFormaPago'  is null.");
            }
            var vMFormaPago = await _context.VMFormaPago.FindAsync(id);
            if (vMFormaPago != null)
            {
                _context.VMFormaPago.Remove(vMFormaPago);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMFormaPagoExists(int id)
        {
          return _context.VMFormaPago.Any(e => e.Id_FormaPago == id);
        }
    }
}
