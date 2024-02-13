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
using Newtonsoft.Json;

namespace MVCVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly MVCVentasContext _context;

        public VentasController(MVCVentasContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lógica para obtener la lista de artículos:

            var listaArticulos = await _context.VMArticle
                .Include(a => a.Precio)
                .Where(a => a.Activo == true 
                        && a.Precio != null)
                .Select(a => new
            {
                Value = a.Id_Articulo.ToString(),
                Text = a.Nombre,
                Precio = a.Precio.Precio
            })
            .ToListAsync();
            // Convierte la lista de artículos a formato JSON.
            var jsonListaArticulos = JsonConvert.SerializeObject(listaArticulos);
            // Pasa la lista de artículos a la vista como una cadena JSON.
            ViewData["JsonListaArticulos"] = jsonListaArticulos;

            // Lógica para obtener la lista de clientes:
            var listaClientes = await _context.VMCliente
                .Include(c => c.Provincia)
                .Include(c => c.Localidad)
                .ToListAsync();
            ViewData["ListaClientes"] = new SelectList(listaClientes, "CodCliente", "RazonSocial");

            //Lógica para obtener la lista de Medio de Pago:
            var listaMedioPago = await _context.VMFormaPago
                .ToListAsync();
            ViewData["ListaMedioPago"] = new SelectList(listaMedioPago, "Id_FormaPago", "Nombre");

            // Lógica para obtener la lista de provincias y localidades:
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "Nombre");
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "Nombre");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCliente([Bind("CodCliente,CUIT,RazonSocial,Nombre,Telefono,Email,Direccion,CodProvincia,CodLocalidad,FechaAlta")] VMCliente vMCliente)
        {
            if (ModelState.IsValid)
            {
                vMCliente.FechaAlta = DateTime.Now;
                vMCliente.CodCliente = vMCliente.CUIT;

                _context.Add(vMCliente);
                await _context.SaveChangesAsync();
                
                var ultimoCliente = await _context.VMCliente
                    .Where(c => c.CodCliente == vMCliente.CodCliente)
                    .FirstOrDefaultAsync();

                var clienteResponse = new { codCliente = ultimoCliente.CodCliente, nombre = ultimoCliente.RazonSocial };
                
                ModelState.Clear();

                return Json(new { success = true, cliente = clienteResponse});
            }
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "CodLocalidad", vMCliente.CodLocalidad);
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "CodProvincia", vMCliente.CodProvincia);
            return View(vMCliente);
        }
    }
}
