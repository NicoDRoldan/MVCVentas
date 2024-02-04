using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class PromoDescuento_EController : Controller
    {
        private readonly MVCVentasContext _context;
        private readonly IMemoryCache _memoryCache;

        public PromoDescuento_EController(MVCVentasContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
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
                .Include(v => v.ListPromoDescuento_D)
                    .ThenInclude(v => v.Articulo)
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
            // Si el id es nulo o la entidad VMPromoDescuento_E es nula, se retorna NotFound.
            if (id == null || _context.VMPromoDescuento_E == null)
            {
                return NotFound();
            }

            // Declarar una variable de tipo VMPromoDescuento_E y asignarle el valor de la promoción con el id correspondiente.
            var vMPromoDescuento_E = await _context.VMPromoDescuento_E
                    .Include(v => v.ListPromoDescuento_D)
                        .ThenInclude(v => v.Articulo)
                    .FirstOrDefaultAsync(v => v.Id_Promocion == id);

            // Asignar a la propiedad ListaArticulos de la promoción, la lista de artículos existentes.
            vMPromoDescuento_E.ListaArticulos = _context.VMArticle
                                .Select(a => new SelectListItem { Value = a.Id_Articulo.ToString(), Text = a.Nombre })
                                .ToList();

            // Asignar a la propiedad ArticulosSeleccionados de la promoción, la lista de artículos seleccionados.
            vMPromoDescuento_E.ArticulosSeleccionados = vMPromoDescuento_E.ListPromoDescuento_D.Select(a => a.Id_Articulo).ToList();

            // Serializar el objeto VMPromoDescuento_E y guardarlo en ViewData.
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                DefaultBufferSize = 65536 // 64 kilobytes
            };
            var jsonModel = JsonSerializer.Serialize(vMPromoDescuento_E, options);
            ViewData["JsonModel"] = jsonModel;

            // Si la promoción es nula, se retorna NotFound.
            if (vMPromoDescuento_E == null)
            {
                return NotFound();
            }

            // Se retorna la vista con la promoción correspondiente.
            ViewData["Id_Tipo"] = new SelectList(_context.Set<VMTipoPromoDescuento>(), "Id_Tipo", "Descripcion", vMPromoDescuento_E.Id_Tipo);
            return View(vMPromoDescuento_E);
        }

        // POST: PromoDescuento_E/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Promocion,Nombre,Porcentaje,FechaInicio,FechaFin,Id_Tipo,ArticulosSeleccionados")] VMPromoDescuento_E vMPromoDescuento_E)
        {
            if (id != vMPromoDescuento_E.Id_Promocion)
            {
                return NotFound();
            }

            // Si el modelo es válido, se intenta actualizar la promoción.
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMPromoDescuento_E);

                    // Si la lista de artículos seleccionados no está vacía, se recorre el foreach.
                    if (vMPromoDescuento_E.ArticulosSeleccionados != null && vMPromoDescuento_E.ArticulosSeleccionados.Any())
                    {
                        var promosDescuentos_D = _context.VMPromoDescuento_D.Where(p => p.Id_Promocion == vMPromoDescuento_E.Id_Promocion).ToList();
                        _context.RemoveRange(promosDescuentos_D);

                        // Se recorre el foreach para insertar los artículos seleccionados en la base.
                        foreach (int idArticulo in vMPromoDescuento_E.ArticulosSeleccionados)
                        {
                            // Recorre el primer artículo, crea una nueva instancia de VMPromoDescuento_D y se le asigna el valor de la promoción creada anteriormente y el id del artículo recorrido.
                            VMPromoDescuento_D vMPromoDescuento_D = new VMPromoDescuento_D
                            {
                                Id_Promocion = vMPromoDescuento_E.Id_Promocion,
                                Id_Articulo = idArticulo
                            };
                            // Se inserta en la base el objeto instanciado.
                            _context.Add(vMPromoDescuento_D);
                        }
                    }
                    else {
                        var promosDescuentos_D = _context.VMPromoDescuento_D.Where(p => p.Id_Promocion == vMPromoDescuento_E.Id_Promocion).ToList();
                        _context.RemoveRange(promosDescuentos_D);
                    }
                    // Se guardan los cambios en la base.
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

            var vMPromoDescuento_E = await _context.VMPromoDescuento_E
                .Include(v => v.ListPromoDescuento_D)
                    .ThenInclude(v => v.Articulo)
                .FirstOrDefaultAsync(v => v.Id_Promocion == id);
            
            if (vMPromoDescuento_E.ListPromoDescuento_D != null && vMPromoDescuento_E.ListPromoDescuento_D.Any())
            {
                TempData["MensajeError"] = "No se puede eliminar la promoción, ya que tiene artículos asociados.";
                return RedirectToAction("Delete");
            }

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
