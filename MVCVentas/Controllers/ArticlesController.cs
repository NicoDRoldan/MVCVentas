using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly MVCVentasContext _context;
        private readonly IMemoryCache _memoryCache;

        public ArticlesController(MVCVentasContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            return _context.VMArticle != null ?
                        View(await _context.VMArticle
                        .Include(v => v.Rubro)
                        .Include(v => v.Precio)
                        .ToListAsync()) : 
                        Problem("Entity set 'MVCVentasContext.VMUser'  is null.");
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMArticle = await _context.VMArticle
                .Include(v => v.Rubro)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);
            if (vMArticle == null)
            {
                return NotFound();
            }

            return View(vMArticle);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewData["Id_Rubro"] = new SelectList(_context.Set<VMRubro>(), "Id_Rubro", "Nombre");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Articulo,Nombre,Id_Rubro,Activo,Descripcion,Fecha,UsaStock,Precio,Stock")] VMArticle vMArticle)
        {
            vMArticle.Fecha = DateTime.Now;
            vMArticle.Precio.Fecha = DateTime.Now;

            // Verifica si el campo Precio.Precio es null antes de agregarlo al contexto
            if (vMArticle.Precio != null && vMArticle.Precio.Precio == null)
            {
                vMArticle.Precio = null; // Setea a null para evitar la inserción en la tabla de precios
            }

            if (!vMArticle.UsaStock)
            {
                vMArticle.Stock = null; // Setea a null para evitar la inserción en la tabla de stock
            }

            ModelState.Clear();
            TryValidateModel(vMArticle);

            if (ModelState.IsValid)
            {
                _context.Add(vMArticle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id_Rubro"] = new SelectList(_context.Set<VMRubro>(), "Id_Rubro", "Id_Rubro", vMArticle.Id_Rubro);
            return View(vMArticle);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            // Relación de Precios con Artículos para mostrar el precio actual del artículo en la vista.
            var vMArticle = await _context.VMArticle
                .Include(a => a.Precio)
                .Include(a => a.Stock)
                .FirstOrDefaultAsync(p => p.Id_Articulo == id);

            //Guarda el valor de fecha en memoria cache en un campo llamado "FechaEdicion".
            _memoryCache.Set("FechaEdicion", vMArticle.Fecha, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            if (vMArticle == null)
            {
                return NotFound();
            }

            ViewData["Id_Rubro"] = new SelectList(_context.Set<VMRubro>(), "Id_Rubro", "Nombre", vMArticle.Id_Rubro);

            return View(vMArticle);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Articulo,Nombre,Id_Rubro,Activo,Descripcion,UsaStock,Precio,Stock")] VMArticle vMArticle)
        {
            if (id != vMArticle.Id_Articulo)
            {
                return NotFound();
            }

            vMArticle.Stock.Id_Articulo = id;

            ModelState.Clear();
            TryValidateModel(vMArticle);

            // Verificar si el modelo es valido.
            if (ModelState.IsValid)
            {
                try
                {
                    // Traer el artículo existente
                    var artExistente = await _context.VMArticle
                        .Include(a => a.Precio)
                        .Include(a => a.Stock)
                        .FirstOrDefaultAsync(a => a.Id_Articulo == vMArticle.Id_Articulo);

                    // Si el artículo no existe devuelve NotFound().
                    if(artExistente == null)
                    {
                        return NotFound();
                    }

                    #region Inicio lógica de Precio

                    // Si el precio del artículo existente, existe, actualizar todos los campos al valor que le pasamos por la vista.
                    if (artExistente.Precio != null)
                    {
                        // Si el precio de la vista es diferente a null, se le asigna el precio pasado por la vista.
                        if (vMArticle.Precio != null)
                        {
                            artExistente.Precio.Precio = vMArticle.Precio.Precio;

                            //Se trae por memoria cache la fecha guardada en el GET del EDIT (Fecha de creación de artículo).
                            if(_memoryCache.TryGetValue("FechaEdicion", out DateTime fechaEdicion))
                            {
                                artExistente.Precio.Fecha = fechaEdicion;
                            }
                        }
                    }
                    // Si el precio no existe, se creará un nuevo objeto y se insertará en la tabla correspondiente con los datos pasados en la vista y un dato predeterminado (fecha).
                    else if (artExistente.Precio == null && vMArticle.Precio.Precio != null)
                    {
                        artExistente.Precio = new VMPrice
                        {
                            Precio = vMArticle.Precio.Precio,
                            Fecha = DateTime.Now
                        };
                    }
                    else if(vMArticle.Precio.Precio == null)
                    {
                        vMArticle.Precio = null;
                    }

                    #endregion Fin Lógica de Precio

                    #region Inicio lógica de Stock
                    if (artExistente.UsaStock)
                    {
                        // Sigue usando Stock?
                        if (vMArticle.UsaStock)
                        {
                            artExistente.Stock.Cantidad = vMArticle.Stock.Cantidad;
                        }
                        else
                        {
                            var stockPorId = await _context.VMStock.FindAsync(artExistente.Id_Articulo);

                            if(stockPorId != null)
                            {
                                _context.Remove(stockPorId);
                                artExistente.Stock = null;
                            }
                        }
                    }
                    else
                    {
                        // No tiene Stock y quiere empezar a usarlo en el artículo.
                        if(vMArticle.UsaStock)
                        {
                            artExistente.Stock = new VMStock
                            {
                                Cantidad = vMArticle.Stock.Cantidad
                            };
                        }
                    }
                    #endregion

                    artExistente.Nombre = vMArticle.Nombre;
                    artExistente.Id_Rubro = vMArticle.Id_Rubro;
                    artExistente.Activo = vMArticle.Activo;
                    artExistente.Descripcion = vMArticle.Descripcion;
                    artExistente.UsaStock = vMArticle.UsaStock;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMArticleExists(vMArticle.Id_Articulo))
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
            ViewData["Id_Rubro"] = new SelectList(_context.Set<VMRubro>(), "Id_Rubro", "Id_Rubro", vMArticle.Id_Rubro);
            return View(vMArticle);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMArticle == null)
            {
                return NotFound();
            }

            var vMArticle = await _context.VMArticle
                .Include(v => v.Rubro)
                .FirstOrDefaultAsync(m => m.Id_Articulo == id);

            if (vMArticle == null)
            {
                return NotFound();
            }

            return View(vMArticle);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMArticle == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMArticle'  is null.");
            }

            var vMArticle = await _context.VMArticle.FindAsync(id);
            var artPrecio = await _context.VMArticle
                .Include(p => p.Precio)
                .Include(s => s.Stock)
                .FirstOrDefaultAsync(a => a.Id_Articulo == id);

            if (vMArticle != null)
            {
                _context.VMArticle.Remove(vMArticle);
            }

            if(artPrecio.Precio != null && artPrecio.Stock != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("No se puede eliminar el artículo ya que el mismo tiene precio y stock asociado.");
                return RedirectToAction("Delete");
            }
            else if(artPrecio.Stock != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("No se puede eliminar el artículo ya que el mismo tiene stock asociado.");
                return RedirectToAction("Delete");
            }
            else if(artPrecio.Precio != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("No se puede eliminar el artículo ya que el mismo tiene un precio asociado.");
                return RedirectToAction("Delete");
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMArticleExists(int id)
        {
          return _context.VMArticle.Any(e => e.Id_Articulo == id);
        }
    }
}
