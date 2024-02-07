using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    public class Comprobantes_NController : Controller
    {
        private readonly MVCVentasContext _context;
        private readonly IMemoryCache _memoryCache;

        public Comprobantes_NController(MVCVentasContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: Comprobantes_N
        public async Task<IActionResult> Index()
        {
            var mVCVentasContext = _context.VMComprobante_N.Include(v => v.Comprobante_E).Include(v => v.Modulo).Include(v => v.Sucursal);
            return View(await mVCVentasContext.ToListAsync());
        }

        // GET: Comprobantes_N/Details/5
        public async Task<IActionResult> Details(string codComprobante, string codModulo, string numSucursal)
        {
            if ((codComprobante == null && codModulo == null && numSucursal == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_N = await _context.VMComprobante_N
                .Include(v => v.Comprobante_E)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo && m.NumSucursal == numSucursal);
            if (vMComprobante_N == null)
            {
                return NotFound();
            }

            return View(vMComprobante_N);
        }

        // GET: Comprobantes_N/Create
        public async Task<IActionResult> Create()
        {
            // En este ViewData se muestra el código de los comprobantes existentes agrupando por el código de comprobante (CodComprobante).
            ViewData["CodComprobante"] = new SelectList(
                _context.VMComprobante_E
                    .GroupBy(c => c.CodComprobante)
                    .Select(g => new SelectListItem
                    {
                        Value = g.Key,
                        Text = $"{g.Key}"
                    }),
                "Value", "Text");

            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo");
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal");
            return View();
        }

        // POST: Comprobantes_N/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodComprobante,CodModulo,NumComprobante,NumSucursal")] VMComprobante_N vMComprobante_N)
        {
            var existeComprobante = await _context.VMComprobante_N
                .FirstOrDefaultAsync(m => m.CodComprobante == vMComprobante_N.CodComprobante && m.CodModulo == vMComprobante_N.CodModulo && m.NumSucursal == vMComprobante_N.NumSucursal);

            if (existeComprobante != null)
            {
                TempData["MensajeError"] = Uri.EscapeDataString("Error, ya existe un comprobante con el mismo código de comprobante, código de módulo y número de sucursal.");
                return RedirectToAction("Create");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vMComprobante_N);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CodComprobante"] = new SelectList(
                _context.VMComprobante_E
                    .GroupBy(c => c.CodComprobante)
                    .Select(g => new SelectListItem
                    {
                        Value = g.Key,
                        Text = $"{g.Key}"
                    }),
                "Value", "Text");

            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_N.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMComprobante_N.NumSucursal);
            return View(vMComprobante_N);
        }

        #region Editar -- No se utiliza en la aplicación
        /*
        // GET: Comprobantes_N/Edit/5
        public async Task<IActionResult> Edit(string codComprobante, string codModulo, string numSucursal)
        {
            // Si no se recibe el código de comprobante, el código de módulo y el número de sucursal, o si el conjunto de entidades VMComprobante_E es nulo,
            // se devuelve un error 404.
            if ((codComprobante == null && codModulo == null && numSucursal == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            // Se busca el comprobante con el código de comprobante, el código de módulo y el número de sucursal recibidos.
            var vMComprobante_N = await _context.VMComprobante_N
                    .Include(v => v.Comprobante_E)
                    .Include(v => v.Modulo)
                    .Include(v => v.Sucursal)
                    .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo && m.NumSucursal == numSucursal);

            // Guarda el número de sucursal original en la memoria caché.
            _memoryCache.Set("NumSucursalOriginal"
                                , vMComprobante_N.NumSucursal,
                                new MemoryCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                                });

            // Si el comprobante no existe, se devuelve un error 404.
            if (vMComprobante_N == null)
            {
                return NotFound();
            }

            // Se crea un SelectList con los códigos de comprobante existentes agrupados por el código de comprobante (CodComprobante).
            ViewData["CodComprobante"] = new SelectList(
                    _context.VMComprobante_E
                        .GroupBy(c => c.CodComprobante)
                        .Select(g => new SelectListItem
                        {
                            Value = g.Key,
                            Text = $"{g.Key}"
                        }),
                    "Value", "Text");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_N.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMComprobante_N.NumSucursal);

            return View(vMComprobante_N);
        }

        // POST: Comprobantes_N/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string codComprobante, string codModulo, string numSucursal, [Bind("NumSucursal")] VMComprobante_N vMComprobante_N)
        {
            // Si no se recibe el código de comprobante, el código de módulo y el número de sucursal, o si el conjunto de entidades VMComprobante_E es nulo,
            // se devuelve un error 404.
            if ((codComprobante == null && codModulo == null && numSucursal == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            if (VMComprobante_NExists(codComprobante, codModulo, numSucursal))
            {
                TempData["MensajeError"] = Uri.EscapeDataString("Error, ya existe un comprobante con el mismo código de comprobante, código de módulo y número de sucursal.");
                return RedirectToAction("Edit");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_memoryCache.TryGetValue("NumSucursalOriginal", out string numSucursalOriginal))
                    {
                        var entidadSucAnterior = await _context.VMComprobante_N
                            .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo && m.NumSucursal == numSucursalOriginal);

                        if (entidadSucAnterior != null)
                        {
                            entidadSucAnterior.NumSucursal = vMComprobante_N.NumSucursal;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMComprobante_NExists(vMComprobante_N.CodComprobante, vMComprobante_N.CodModulo, vMComprobante_N.NumSucursal))
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

            ViewData["CodComprobante"] = new SelectList(
                _context.VMComprobante_E
                    .GroupBy(c => c.CodComprobante)
                    .Select(g => new SelectListItem
                    {
                        Value = g.Key,
                        Text = $"{g.Key}"
                    }),
                "Value", "Text");
            ViewData["CodModulo"] = new SelectList(_context.VMModulo, "CodModulo", "CodModulo", vMComprobante_N.CodModulo);
            ViewData["NumSucursal"] = new SelectList(_context.VMSucursal, "NumSucursal", "NumSucursal", vMComprobante_N.NumSucursal);

            return View(vMComprobante_N);
        }
        */
        #endregion

        // GET: Comprobantes_N/Delete/5
        public async Task<IActionResult> Delete(string codComprobante, string codModulo, string numSucursal)
        {
            if ((codComprobante == null && codModulo == null && numSucursal == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_N = await _context.VMComprobante_N
                .Include(v => v.Comprobante_E)
                .Include(v => v.Modulo)
                .Include(v => v.Sucursal)
                .FirstOrDefaultAsync(m => m.CodComprobante == codComprobante && m.CodModulo == codModulo && m.NumSucursal == numSucursal);

            if (vMComprobante_N == null)
            {
                return NotFound();
            }

            return View(vMComprobante_N);
        }

        // POST: Comprobantes_N/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string codComprobante, string codModulo, string numSucursal)
        {
            if ((codComprobante == null && codModulo == null && numSucursal == null) || (_context.VMComprobante_E == null))
            {
                return NotFound();
            }

            var vMComprobante_N = await _context.VMComprobante_N.FindAsync(codComprobante, codModulo, numSucursal);

            if (vMComprobante_N != null)
            {
                _context.VMComprobante_N.Remove(vMComprobante_N);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool VMComprobante_NExists(string codComprobante, string codModulo, string numSucursal)
        {
            return _context.VMComprobante_N.Any(e => e.CodComprobante == codComprobante && e.CodModulo == codModulo && e.NumSucursal == numSucursal);
        }

        [HttpGet]
        public async Task<JsonResult> GetModulosByComprobante(string CodComprobante)
        {
            var modulos = await _context.VMComprobante_E
                .Where(e => e.CodComprobante == CodComprobante)
                .Select(e => e.CodModulo)
                .Distinct()
                .ToListAsync();

            return Json(modulos);
        }
    }
}
