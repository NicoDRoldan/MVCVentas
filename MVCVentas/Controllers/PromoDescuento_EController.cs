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
    public class PromoDescuento_EController : Controller
    {
        private readonly MVCVentasContext _context;

        public PromoDescuento_EController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: PromoDescuento_E
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMPromoDescuento_E.Include(v => v.TipoPromoDescuento);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: PromoDescuento_E/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMPromoDescuento_E == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_E = await _context.VMPromoDescuento_E
                .Include(v => v.TipoPromoDescuento)
                .FirstOrDefaultAsync(m => m.Id_Promocion == id);

            if (vMPromoDescuento_E == null)
            {
                return NotFound();
            }

            return View(vMPromoDescuento_E);
        }

        // GET: PromoDescuento_E/Create
        public IActionResult Create()
        {
            /* Se guardan los artículos existentes en ListaArticulos perteneciente al modelo PromoDescuento_E,
            dicha lista es mostrada en el Create correspondiente*/

            var model = new VMPromoDescuento_E
            {
                ListaArticulos = _context.VMArticle
                                    .Select(a => new SelectListItem { Value = a.Id_Articulo.ToString(), Text = a.Nombre })
                                    .ToList()
            };

            ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Descripcion");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Promocion,Nombre,Porcentaje,FechaInicio,FechaFin,Id_Tipo,ArticulosSeleccionados")] 
                                                    VMPromoDescuento_E vMPromoDescuento_E)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Se inserta antes la promoción, para así poder asignarle un ID a PromosDescuentos_D.

                    _context.Add(vMPromoDescuento_E);

                    await _context.SaveChangesAsync();

                    // Si la lista de artículos seleccionados no está vacía, se recorre el foreach.

                    if (vMPromoDescuento_E.ArticulosSeleccionados != null && vMPromoDescuento_E.ArticulosSeleccionados.Any())
                    {
                        foreach (int idArticulo in vMPromoDescuento_E.ArticulosSeleccionados)
                        {
                            /* Recorre el primer artícuulo, crea una nueva instancia de VMPromoDescuento_D y se le asigna el 
                            ID de la promoción creada anteriormente y el id del artículo recorrido. */

                            VMPromoDescuento_D vMPromoDescuento_D = new VMPromoDescuento_D
                            {
                                Id_Promocion = vMPromoDescuento_E.Id_Promocion,
                                Id_Articulo = idArticulo
                            };

                            // Se inserta en la base el objeto instanciado.

                            _context.Add(vMPromoDescuento_D);
                        }

                        // Se guardan los cambios en la base.

                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Id_Tipo", vMPromoDescuento_E.Id_Tipo);
                    return View(vMPromoDescuento_E);
                }
            }
            ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Id_Tipo", vMPromoDescuento_E.Id_Tipo);
            return View(vMPromoDescuento_E);
        }

        // GET: PromoDescuento_E/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMPromoDescuento_E == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_E = await _context.VMPromoDescuento_E.FindAsync(id);
            if (vMPromoDescuento_E == null)
            {
                return NotFound();
            }
            ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Id_Tipo", vMPromoDescuento_E.Id_Tipo);
            return View(vMPromoDescuento_E);
        }

        // POST: PromoDescuento_E/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Promocion,Nombre,Porcentaje,FechaInicio,FechaFin,Id_Tipo")] VMPromoDescuento_E vMPromoDescuento_E)
        {
            if (id != vMPromoDescuento_E.Id_Promocion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMPromoDescuento_E);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMPromoDescuento_EExists(vMPromoDescuento_E.Id_Promocion))
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
            ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Id_Tipo", vMPromoDescuento_E.Id_Tipo);
            return View(vMPromoDescuento_E);
        }

        // GET: PromoDescuento_E/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMPromoDescuento_E == null)
            {
                return NotFound();
            }

            var vMPromoDescuento_E = await _context.VMPromoDescuento_E
                .Include(v => v.TipoPromoDescuento)
                .FirstOrDefaultAsync(m => m.Id_Promocion == id);
            if (vMPromoDescuento_E == null)
            {
                return NotFound();
            }

            return View(vMPromoDescuento_E);
        }

        // POST: PromoDescuento_E/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMPromoDescuento_E == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMPromoDescuento_E'  is null.");
            }
            var vMPromoDescuento_E = await _context.VMPromoDescuento_E.FindAsync(id);
            if (vMPromoDescuento_E != null)
            {
                _context.VMPromoDescuento_E.Remove(vMPromoDescuento_E);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMPromoDescuento_EExists(int id)
        {
          return _context.VMPromoDescuento_E.Any(e => e.Id_Promocion == id);
        }
    }
}
