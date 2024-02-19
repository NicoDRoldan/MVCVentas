using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MVCVentas.Data;
using MVCVentas.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Font = iTextSharp.text.Font;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly MVCVentasContext _context;

        public VentasController(MVCVentasContext context)
        {
            _context = context;
        }

        string rutaRaizApp = AppDomain.CurrentDomain.BaseDirectory;

        public async Task<IActionResult> Index()
        {
            // Lógica para obtener la lista de artículos:

            var listaArticulos = await _context.VMArticle
                .Include(a => a.Precio)
                .Include(a => a.Rubro)
                .Where(a => a.Activo == true 
                        && a.Precio != null
                        && a.Precio.Precio > 0)
                .Select(a => new
            {
                Value = a.Id_Articulo.ToString(),
                Text = a.Nombre,
                Precio = a.Precio.Precio,
                Rubro = a.Id_Rubro.ToString()
            })
            .ToListAsync();
            // Convierte la lista de artículos a formato JSON.
            var jsonListaArticulos = JsonConvert.SerializeObject(listaArticulos);
            // Pasa la lista de artículos a la vista como una cadena JSON.
            ViewData["JsonListaArticulos"] = jsonListaArticulos;

            // Lógica para obtener la lista de Rubros:
            var listaRubros = await _context.VMRubro
                .Join(_context.VMArticle,
                    r => r.Id_Rubro,
                    a => a.Id_Rubro,
                    (r, a) => new { Articulo = a, Rubro = r })
                .Where(ar => ar.Articulo.Activo == true
                        && ar.Articulo.Precio != null
                        && ar.Articulo.Precio.Precio > 0)
                .GroupBy(ar => new { ar.Rubro.Id_Rubro, ar.Rubro.Nombre })
                .Select(grp => new
                {
                    Value = grp.Key.Id_Rubro.ToString(),
                    Text = grp.Key.Nombre
                })
                .ToListAsync();
            // Convierte la lista de rubros a formato JSON.
            var jsonListaRubros = JsonConvert.SerializeObject(listaRubros);
            ViewData["JsonListaRubros"] = jsonListaRubros;

            // Lógica para obtener la lista de clientes:
            var listaClientes = await _context.VMCliente
                .Include(c => c.Provincia)
                .Include(c => c.Localidad)
                .ToListAsync();
            ViewData["ListaClientes"] = new SelectList(listaClientes, "CodCliente", "RazonSocial");

            // Lógica para obtener la lista de comprobantes:
            var listaComprobantes = await _context.VMComprobante_E
                .GroupBy(c => c.CodComprobante)
                .Select(c => c.FirstOrDefault())
                .ToListAsync();
            ViewData["ListaComprobantes"] = new SelectList(listaComprobantes, "CodComprobante", "Nombre", "FAB");

            // Lógica para obtener la lista de Medio de Pago:
            var listaMedioPago = await _context.VMFormaPago
                .ToListAsync();
            ViewData["ListaMedioPago"] = new SelectList(listaMedioPago, "Id_FormaPago", "Nombre");

            // Lógica para obtener la lista de Módulos:
            var listaModulos = await _context.VMModulo
                .ToListAsync();
            ViewData["ListaModulosVTAS"] = new SelectList(listaModulos, "CodModulo", "Descripcion", "VTAS");

            // Lógica para obtener la lista de provincias y localidades:
            ViewData["CodLocalidad"] = new SelectList(_context.Set<VMLocalidad>(), "CodLocalidad", "Nombre");
            ViewData["CodProvincia"] = new SelectList(_context.Set<VMProvincia>(), "CodProvincia", "Nombre");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCliente([Bind("CodCliente,CUIT,RazonSocial,Nombre,Telefono,Email,Direccion,CodProvincia,CodLocalidad,FechaAlta")] 
                                                                                                                                                VMCliente vMCliente)
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

        [HttpPost]
        public async Task<IActionResult> AltaComprobanteN([Bind("NumSucursal,CodComprobante,CodModulo,NumComprobante")] 
                                                                                                VMComprobante_N vMComprobante_N)
        {
            if (ModelState.IsValid)
            {
                vMComprobante_N.NumComprobante = 0;
                _context.Add(vMComprobante_N);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<decimal> ObtenerNumVenta(string numSucursal, string codComprobante, string codModulo)
        {
            var comprobante = await _context.VMComprobante_N
                .Where(c =>
                    c.NumSucursal == numSucursal
                    && c.CodComprobante == codComprobante
                    && c.CodModulo == codModulo)
                .FirstOrDefaultAsync();

            if(comprobante == null)
            {
                return -1;
            }

            return comprobante.NumComprobante;
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta(VMVentas vMVentas, 
                                                    List<VMVentasDetalle> detallesventa, 
                                                    VMVentaImporte vMVentaImporte)
        {
            // Datos Generales:

            // Traer Comprobante_N:
            vMVentas.Comprobante_E = await _context.VMComprobante_E
                .Where(c => c.CodComprobante == vMVentas.CodComprobante
                        && c.CodModulo == vMVentas.CodModulo)
                .FirstOrDefaultAsync();

            // Traer Modulo:
            vMVentas.Modulo = await _context.VMModulo
                .Where(m => m.CodModulo == vMVentas.CodModulo)
                .FirstOrDefaultAsync();

            // Traer Sucursal:
            vMVentas.Sucursal = await (from s in _context.VMSucursal
                                       join c in _context.VMConfig on s.NumSucursal equals c.Valor_Config
                                       where c.Codigo_Config == "Sucursal_Config"
                                       select s).FirstOrDefaultAsync();

            // Datos Ventas E:

            // Traer Número de Caja:
            var numCajaString = await _context.VMConfig
                .Where(c => c.Codigo_Config == "NumCaja_Config")
                .Select(c => c.Valor_Config)
                .FirstOrDefaultAsync();

            if (int.TryParse(numCajaString, out int numCaja))
            {
                vMVentas.NumCaja = numCaja;
            }
            else
            {
                return Json(new { success = false, message = "Error al obtener el número de caja." });
            }

            // Traer Forma de Pago:
            vMVentas.FormaPago = await _context.VMFormaPago
                .Where(f => f.Id_FormaPago == vMVentas.Id_FormaPago)
                .FirstOrDefaultAsync();

            // Traer Cliente:
            vMVentas.Cliente = await _context.VMCliente
                .Where(c => c.CodCliente == vMVentas.CodCliente)
                .FirstOrDefaultAsync();

            // Traer Usuario:
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            vMVentas.Usuario = await _context.VMUser
                .Where(u => u.Usuario == userId)
                .FirstOrDefaultAsync();

            // Datos Ventas D:
            
            int renglon = 1; // Inicialización de Renglon:

            // Datos Ventas I:

            decimal impSubTotal = 0;
            vMVentaImporte.Descuento = vMVentaImporte.Descuento.Replace(".", ","); // Se reemplaza el punto por la coma.

            decimal descuento = decimal.Parse(vMVentaImporte.Descuento);

            var conceptos = await _context.VMConcepto
                .ToListAsync();

            decimal iva21 = 1.21M;


            // Obtener el número de venta:
            decimal nroVenta = await ObtenerNumVenta(vMVentas.Sucursal.NumSucursal,
                vMVentas.Comprobante_E.CodComprobante,
                vMVentas.Modulo.CodModulo);

            // Si no se pudo obtener el número de venta, se da de alta un nuevo comprobante:
            if (nroVenta == -1)
            {
                var altaComprobanteResponse = await AltaComprobanteN(new VMComprobante_N
                {
                    NumSucursal = vMVentas.Sucursal.NumSucursal,
                    CodComprobante = vMVentas.Comprobante_E.CodComprobante,
                    CodModulo = vMVentas.Modulo.CodModulo
                }) as JsonResult;

                if(altaComprobanteResponse != null)
                {
                    dynamic altaComprobanteData = altaComprobanteResponse.Value;
                    if (altaComprobanteData.success == true)
                    {
                        nroVenta = 0;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Error al obtener el número de venta." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Error al obtener el número de venta." });
                }
            }
            
            // Se incrementa el número de venta:
            decimal nroVentaDecimal = nroVenta + 1;
            // Se convierte el número de venta a string y se le agrega ceros a la izquierda:
            string nroVentaString = nroVentaDecimal.ToString();
            string nroVentaCorrelativa = nroVentaString.PadLeft(8, '0');

            // Se inicia la transacción:
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Se da de alta la venta:
                    var ventaE = new VMVentas_E
                    {
                        NumVenta = nroVentaCorrelativa, // Se obtiene con la función de obtener número de venta. OK
                        CodComprobante = vMVentas.Comprobante_E.CodComprobante, // Se obtiene de la vista. OK
                        CodModulo = vMVentas.Modulo.CodModulo, // Se obtiene de la vista (HardCodeado a VTAS). OK
                        NumSucursal = vMVentas.Sucursal.NumSucursal, // Se obtiene de la base (Tabla Config). OK
                        Fecha = DateTime.Now, // Se obtiene del sistema. OK
                        Hora = DateTime.Now, // Se obtiene del sistema. OK
                        id_FormaPago = vMVentas.FormaPago.Id_FormaPago, // Se obtiene de la vista. OK
                        CodCliente = vMVentas.Cliente.CodCliente, // Se obtiene de la vista. OK
                        id_Usuario = vMVentas.Usuario.Id_Usuario, // Se obtiene de la sesión. OK
                        NumCaja = vMVentas.NumCaja // Se obtiene de la base (Tabla Config). OK
                    };
                    // Se da de alta la venta en la base de datos:
                    _context.VMVentas_E.Add(ventaE);

                    // Se da de alta los detalles de la venta:
                    foreach (var detalle in detallesventa)
                    {
                        if (detalle.Id_Articulo == null)
                        {
                            continue;
                        }

                        var articulo = await _context.VMArticle
                            .Where(a => a.Id_Articulo == int.Parse(detalle.Id_Articulo))
                            .FirstOrDefaultAsync();

                        detalle.PrecioUnitario = detalle.PrecioUnitario.Replace(".", ","); // Se reemplaza el punto por la coma.

                        var ventaD = new VMVentas_D
                        {
                            NumVenta = nroVentaCorrelativa, // Se obtiene con la función de obtener número de venta. OK
                            CodComprobante = vMVentas.Comprobante_E.CodComprobante, // Se obtiene de la vista. OK
                            CodModulo = vMVentas.Modulo.CodModulo, // Se obtiene de la vista (HardCodeado a VTAS). OK
                            NumSucursal = vMVentas.Sucursal.NumSucursal, // Se obtiene de la base (Tabla Config). OK
                            Renglon = renglon, // Se incrementa en cada iteración. OK
                            Id_Articulo = int.Parse(detalle.Id_Articulo), // Se obtiene de la vista. OK
                            Cantidad = int.Parse(detalle.Cantidad), // Se obtiene de la vista. OK
                            Detalle = articulo.Nombre, // Se obtiene de la base de datos. OK
                            PrecioUnitario = decimal.Parse(detalle.PrecioUnitario), // Se obtiene de la vista. OK
                            PrecioTotal = int.Parse(detalle.Cantidad) * decimal.Parse(detalle.PrecioUnitario) // Se calcula acá. OK
                        };
                        // Se da de alta el detalle de la venta en la base de datos:
                        _context.VMVentas_D.Add(ventaD);
                        // Se incrementa el renglón:
                        renglon++;

                        // Se calcula el importe subtotal:
                        impSubTotal += ventaD.PrecioTotal;
                    }

                    // Se da de alta el importe de la venta:
                    foreach (var concepto in conceptos)
                    {
                        if(descuento == 0 && concepto.CodConcepto == "DTO")
                        {
                            continue;
                        }

                        decimal importe = 0;
                        decimal neto = 0;
                        decimal iva = 0;
                        decimal porcentajeDescuento = 0;

                        switch (concepto.CodConcepto)
                        {
                            case "SUBTOTAL":
                                importe = impSubTotal;
                                break;
                            case "DTO":
                                importe = descuento;
                                porcentajeDescuento = (descuento / impSubTotal) * 100;
                                break;
                            case "NETO1":
                                importe = (impSubTotal - descuento) / iva21;
                                break;
                            case "IVA21":
                                neto = importe = (impSubTotal - descuento) / iva21;
                                importe = impSubTotal - descuento - neto;
                                break;
                            case "TOTAL":
                                neto = importe = (impSubTotal - descuento) / iva21;
                                iva = impSubTotal - descuento - neto;
                                importe = neto + iva;
                                break;
                        }

                        var ventaI = new VMVentas_I
                        {
                            NumVenta = nroVentaCorrelativa, // Se obtiene con la función de obtener número de venta. OK
                            CodComprobante = vMVentas.Comprobante_E.CodComprobante, // Se obtiene de la vista. OK
                            CodModulo = vMVentas.Modulo.CodModulo, // Se obtiene de la vista (HardCodeado a VTAS). OK
                            NumSucursal = vMVentas.Sucursal.NumSucursal, // Se obtiene de la base (Tabla Config). OK
                            CodConcepto = concepto.CodConcepto, // Se obtiene de la base de datos. OK
                            Importe = importe, // Se calcula en el switch dependiendo el concepto. OK
                            Descuento = porcentajeDescuento // Se calcula en el switch. OK
                        };
                        // Se da de alta el importe de la venta en la base de datos:
                        _context.VMVentas_I.Add(ventaI);
                    }

                    // Se guardan los cambios en la base de datos:
                    await _context.SaveChangesAsync();

                    // Se trae Comprobante_N
                    var comprobanteN = await _context.VMComprobante_N
                        .Where(c => 
                            c.NumSucursal == vMVentas.Sucursal.NumSucursal
                            && c.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                            && c.CodModulo == vMVentas.Modulo.CodModulo)
                        .FirstOrDefaultAsync();

                    if(comprobanteN != null)
                    {
                        // Se actualiza el número de comprobante en Comprobantes_N:
                        comprobanteN.NumComprobante = nroVenta + 1;
                        await _context.SaveChangesAsync();
                    }

                    // Se confirma la transacción:
                    transaction.Commit();

                    var listVentaD = await _context.VMVentas_D
                        .Where(v => v.NumVenta == nroVentaCorrelativa
                                && v.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                                && v.CodModulo == vMVentas.Modulo.CodModulo
                                && v.NumSucursal == vMVentas.Sucursal.NumSucursal)
                        .ToListAsync();

                    var listVentaI = await _context.VMVentas_I
                        .Where(v => v.NumVenta == nroVentaCorrelativa
                                && v.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                                && v.CodModulo == vMVentas.Modulo.CodModulo
                                && v.NumSucursal == vMVentas.Sucursal.NumSucursal)
                        .ToListAsync();

                    GenerarReportePDF(ventaE, listVentaD, listVentaI, rutaRaizApp);
                    ImprimirReporte(ventaE, listVentaD, listVentaI);

                    return Json(new { success = true, message = "\nSe insertó la venta nro: " + nroVentaCorrelativa + " correctamente. \nDetalle de la venta: " + (detallesventa.Count - 1) + " artículos." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        public Printer GenerarReporte(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I)
        {
            // Se crea una instancia de la clase Printer:
            Printer printer = new Printer();

            // Se agregan las líneas al reporte:
            printer.agregarLinea("Comprobante: " + vMVentas_E.Comprobante.Nombre, 7, "Consolas");
            printer.agregarLinea("Número de Venta: " + vMVentas_E.NumVenta, 7, "Consolas");
            printer.agregarLinea("Fecha: " + vMVentas_E.Fecha.ToString("dd/MM/yyyy"), 7, "Consolas");
            printer.agregarLinea("Hora: " + vMVentas_E.Hora.ToString("HH:mm:ss"), 7, "Consolas");
            printer.agregarLinea("Cliente: " + vMVentas_E.Cliente.RazonSocial, 7, "Consolas");
            printer.agregarLinea("Forma De Pago: " + vMVentas_E.FormaPago.Nombre, 7, "Consolas");

            printer.agregarLineaEnBlanco();

            printer.agregarLinea("Detalle de la venta:", 7, "Consolas", FontStyle.Bold);

            foreach(var detalle in vMVentas_D)
            {
                printer.agregarLineaConExtremos(detalle.Articulo.Nombre, detalle.Cantidad.ToString() + " x " + detalle.PrecioUnitario.ToString("0.00") + " = " + detalle.PrecioTotal.ToString("0.00"), 7, "Consolas");
            }

            printer.agregarLineaEnBlanco();

            printer.agregarLinea("Importe de la venta:", 7, "Consolas", FontStyle.Bold);

            foreach(var importe in vMVentas_I)
            {
                printer.agregarLineaConExtremos(importe.Concepto.Descripcion, importe.Importe.ToString("0.00"), 7, "Consolas");
            }

            return printer;
        }

        public void ImprimirReporte(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I)
        {
            Printer printer = GenerarReporte(vMVentas_E, vMVentas_D, vMVentas_I);

            printer.Imprimir();
        }

        public void GenerarReportePDF(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I, string projectPath)
        {
            string rutaComprobantesImpresos = Path.Combine(projectPath, "Comprobantes Impresos");
            string rutaCodComprobate = Path.Combine(rutaComprobantesImpresos, vMVentas_E.CodComprobante);
            string rutaNumSucursal = Path.Combine(rutaCodComprobate, vMVentas_E.NumSucursal);
            string rutaReporte = Path.Combine(rutaNumSucursal, vMVentas_E.NumVenta + ".pdf");

            Directory.CreateDirectory(rutaComprobantesImpresos);
            Directory.CreateDirectory(rutaCodComprobate);
            Directory.CreateDirectory(rutaNumSucursal);

            Printer printer = GenerarReporte(vMVentas_E, vMVentas_D, vMVentas_I);

            Document document = new Document();

            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaReporte, FileMode.Create));

            document.SetMargins(0f, 0f, 0f, 0f);

            document.Open();

            foreach (var linea in printer.ObtenerLineas())
            {
                Font font = FontFactory.GetFont(linea.fuente, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED, linea.tamano, (int)linea.estilo);
                Paragraph paragraph = new Paragraph(linea.texto, font);

                document.Add(paragraph);
            }

            document.Close();
        }
    }
}