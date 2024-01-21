﻿using System;
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
                          View(await _context.VMUser.ToListAsync()) :
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
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Usuario,Id_Categoria,Usuario,Password,Estado,Fecha,Nombre,Apellido")] VMUser vMUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vMUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vMUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VMUser == null)
            {
                return NotFound();
            }

            var vMUser = await _context.VMUser.FindAsync(id);
            if (vMUser == null)
            {
                return NotFound();
            }
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vMUser);
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
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vMUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VMUser == null)
            {
                return NotFound();
            }

            var vMUser = await _context.VMUser
                .FirstOrDefaultAsync(m => m.Id_Usuario == id);
            if (vMUser == null)
            {
                return NotFound();
            }

            return View(vMUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
