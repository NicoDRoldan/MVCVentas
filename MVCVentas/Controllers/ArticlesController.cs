using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
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
        public async Task<IActionResult> Create([Bind("Id_Articulo,Nombre,Id_Rubro,Activo,Descripcion,Fecha,Precio")] VMArticle vMArticle)
        {
            vMArticle.Fecha = DateTime.Now;
            vMArticle.Precio.Fecha = DateTime.Now;

            // Verifica si el campo Precio.Precio es null antes de agregarlo al contexto
            if (vMArticle.Precio != null && vMArticle.Precio.Precio == null)
            {
                vMArticle.Precio = null; // Setea a null para evitar la inserción en la tabla de precios
            }

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
                .FirstOrDefaultAsync(p => p.Id_Articulo == id);

            //Guarda el valor de fecha en memoria cache en un campo llamado "FechaEdicion".
            _memoryCache.Set("FechaEdicion", vMArticle.Fecha, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id_Articulo,Nombre,Id_Rubro,Activo,Descripcion,Precio")] VMArticle vMArticle)
        {
            if (id != vMArticle.Id_Articulo)
            {
                return NotFound();
            }

            var fechaArt = await _context.VMArticle
                .Where(a => a.Id_Articulo == id)
                .Select(a => a.Fecha)
                .FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    var artExistente = await _context.VMArticle
                        .Include(a => a.Precio)
                        .FirstOrDefaultAsync(a => a.Id_Articulo == vMArticle.Id_Articulo);

                    if(artExistente == null)
                    {
                        return NotFound();
                    }

                    if (artExistente.Precio != null)
                    {
                        artExistente.Nombre = vMArticle.Nombre;
                        artExistente.Id_Rubro = vMArticle.Id_Rubro;
                        artExistente.Activo = vMArticle.Activo;
                        artExistente.Descripcion = vMArticle.Descripcion;

                        if (vMArticle.Precio != null)
                        {
                            artExistente.Precio.Precio = vMArticle.Precio.Precio;

                            if(_memoryCache.TryGetValue("FechaEdicion", out DateTime fechaEdicion))
                            {
                                artExistente.Precio.Fecha = fechaEdicion;
                            }
                        }
                    }
                    else
                    {
                        artExistente.Precio = new VMPrice
                        {
                            Precio = vMArticle.Precio.Precio,
                            Fecha = fechaArt
                        };
                    }

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
            if (vMArticle != null)
            {
                _context.VMArticle.Remove(vMArticle);
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
