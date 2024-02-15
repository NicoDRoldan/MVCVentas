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
    public class ConfigsController : Controller
    {
        private readonly MVCVentasContext _context;

        public ConfigsController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Configs
        public async Task<IActionResult> Index()
        {
              return View(await _context.VMConfig.ToListAsync());
        }

        // GET: Configs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VMConfig == null)
            {
                return NotFound();
            }

            var vMConfig = await _context.VMConfig
                .FirstOrDefaultAsync(m => m.Codigo_Config == id);
            if (vMConfig == null)
            {
                return NotFound();
            }

            return View(vMConfig);
        }

        // GET: Configs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Configs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo_Config,Descripcion_Config,Valor_Config,Fecha_Creacion")] VMConfig vMConfig)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMConfig);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMConfig);
        }

        // GET: Configs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VMConfig == null)
            {
                return NotFound();
            }

            var vMConfig = await _context.VMConfig.FindAsync(id);
            if (vMConfig == null)
            {
                return NotFound();
            }
            return View(vMConfig);
        }

        // POST: Configs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Codigo_Config,Descripcion_Config,Valor_Config,Fecha_Creacion")] VMConfig vMConfig)
        {
            if (id != vMConfig.Codigo_Config)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMConfig);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMConfigExists(vMConfig.Codigo_Config))
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
            return View(vMConfig);
        }

        [HttpGet]
        public async Task<IActionResult> EditConfig(string codigo_Config)
        {
            var config = await _context.VMConfig
                .Where(c => c.Codigo_Config == codigo_Config)
                .FirstOrDefaultAsync();

            if(config != null)
            {
                return Json(new 
                { 
                    success = true, 
                    codigo_Config = config.Codigo_Config, 
                    descripcion_Config = config.Descripcion_Config, 
                    valor_Config = config.Valor_Config 
                });
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfig(string Codigo_Config, [Bind("Codigo_Config,Descripcion_Config,Valor_Config")]
                                                                                                                        VMConfig vMConfig)
        {
            if (Codigo_Config != vMConfig.Codigo_Config)
            {
                return NotFound();
            }

            if(Codigo_Config == "Sucursal_Config")
            {
                var sucursal = await _context.VMSucursal
                    .Where(s => s.NumSucursal == vMConfig.Valor_Config)
                    .FirstOrDefaultAsync();

                if (sucursal == null)
                {
                    return Json(new { success = false, message = "La sucursal no existe." });
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vMConfig.Fecha_Creacion = DateTime.Now;
                    _context.Update(vMConfig);
                    await _context.SaveChangesAsync();

                    switch (vMConfig.Codigo_Config)
                    {
                        case "Sucursal_Config":
                            return Json(new { success = true, message = "Sucursal configurada correctamente" });
                        case "NumCaja_Config":
                            return Json(new { success = true, message = "Número de Caja configurada correctamente" });
                        default:
                            return Json(new { success = true, message = "Configuración actualizada correctamente" });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VMConfigExists(vMConfig.Codigo_Config))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(vMConfig);
        }

        // GET: Configs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VMConfig == null)
            {
                return NotFound();
            }

            var vMConfig = await _context.VMConfig
                .FirstOrDefaultAsync(m => m.Codigo_Config == id);
            if (vMConfig == null)
            {
                return NotFound();
            }

            return View(vMConfig);
        }

        // POST: Configs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VMConfig == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMConfig'  is null.");
            }
            var vMConfig = await _context.VMConfig.FindAsync(id);
            if (vMConfig != null)
            {
                _context.VMConfig.Remove(vMConfig);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMConfigExists(string id)
        {
          return _context.VMConfig.Any(e => e.Codigo_Config == id);
        }
    }
}
