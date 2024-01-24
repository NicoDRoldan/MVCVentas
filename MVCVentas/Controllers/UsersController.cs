using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Data;
using MVCVentas.Models;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly MVCVentasContext _context;

        public UsersController(MVCVentasContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
              return _context.VMUser != null ? 
                          View(await _context.VMUser
                          .Include(u => u.Categoria)
                          .ToListAsync()) :
                          Problem("Entity set 'MVCVentasContext.VMUser'  is null.");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VMUser == null)
            {
                return NotFound();
            }

            var vMUser = await _context.VMUser
                .Include(u => u.Categoria)
                .FirstOrDefaultAsync(m => m.Id_Usuario == id);

            if (vMUser == null)
            {
                return NotFound();
            }

            return View(vMUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Usuario,Id_Categoria,Usuario,Password,Estado,Fecha,Nombre,Apellido")] VMUser vMUser)
        {
            vMUser.Fecha = DateTime.Now;
            vMUser.Estado = true;

            bool usuarioExiste = _context.VMUser
                                    .Include (u => u.Categoria)
                                    .Any(u => u.Usuario == vMUser.Usuario);

            if (usuarioExiste)
            {
                ModelState.AddModelError("Usuario", "El usuario ya existe.");
                ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");
                return View(vMUser);
            }

            if (ModelState.IsValid)
            {
                _context.Add(vMUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");
            return View(vMUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMUser == null)
            {
                return NotFound();
            }

            var vMUser = await _context.VMUser
                .Include(u => u.Categoria)
                .FirstOrDefaultAsync(m => m.Id_Usuario == id);

            if (vMUser == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");
            return View(vMUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Usuario,Id_Categoria,Usuario,Password,Estado,Fecha,Nombre,Apellido")] VMUser vMUser)
        {
            if (id != vMUser.Id_Usuario)
            {
                return NotFound();
            }

            var entidadVmUser = await _context.VMUser
                .Include(u => u.Categoria)
                .FirstOrDefaultAsync(m => m.Id_Usuario == id);

            bool usuarioExiste = _context.VMUser.Any(u => u.Usuario == vMUser.Usuario);

            if ((vMUser.Usuario != entidadVmUser.Usuario) && usuarioExiste)
            {
                ModelState.AddModelError("Usuario", "El usuario ya existe.");
                ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");
                return View(vMUser);
            }

            entidadVmUser.Id_Categoria = vMUser.Id_Categoria;
            entidadVmUser.Usuario = vMUser.Usuario;
            entidadVmUser.Estado = vMUser.Estado;
            entidadVmUser.Nombre = vMUser.Nombre;
            entidadVmUser.Apellido = vMUser.Apellido;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VMUserExists(vMUser.Id_Usuario))
                {
                    return NotFound();
                }
                else
                {
                    ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");

                    return View(vMUser);
                }
            }
            return RedirectToAction(nameof(Index));

        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMUser == null)
            {
                return NotFound();
            }

            var vMUser = await _context.VMUser
                .Include(u => u.Categoria)
                .FirstOrDefaultAsync(m => m.Id_Usuario == id);

            if (vMUser == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = new SelectList(_context.VMCategory.ToList(), "Id_Categoria", "Nombre");

            return View(vMUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VMUser == null)
            {
                return Problem("Entity set 'MVCVentasContext.VMUser'  is null.");
            }
            var vMUser = await _context.VMUser.FindAsync(id);
            if (vMUser != null)
            {
                _context.VMUser.Remove(vMUser);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VMUserExists(int id)
        {
          return (_context.VMUser?.Any(e => e.Id_Usuario == id)).GetValueOrDefault();
        }
    }
}
