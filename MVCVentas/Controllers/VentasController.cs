﻿using System;
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
                Rubro = a.Id_Rubro.ToString(),
                UsaStock = a.UsaStock,
                UsaCombo = a.UsaCombo
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

            // Lógica para obtener los descuentos:
            var listaDescuentos = await _context.VMPromoDescuento_E
                .Where(p => p.FechaInicio <= DateTime.Now
                        && p.FechaFin >= DateTime.Now
                        && p.Id_Tipo == 1)
                .Include(p => p.ListPromoDescuento_D)
                .Include(p => p.TipoPromoDescuento)
                .Select(pd => new
                {
                    Value = pd.Id_Promocion.ToString(),
                    Text = pd.Nombre,
                    Porcentaje = pd.Porcentaje
                })
                .ToListAsync();
            var jsonListDescuentos = JsonConvert.SerializeObject(listaDescuentos);
            ViewData["JsonListaDescuentos"] = jsonListDescuentos;

            // Lógica para obtener las promociones:
            var listaPromociones = await _context.VMPromoDescuento_E
                .Join(_context.VMPromoDescuento_D,
                    pe => pe.Id_Promocion,
                    pd => pd.Id_Promocion,
                    (pe, pd) => new { PromoE = pe, PromoD = pd }
                )
                .Join(_context.VMArticle,
                prd => prd.PromoD.Id_Articulo,
                a => a.Id_Articulo,
                (prd, a) => new { PromoED = prd, Articulo = a }
                )
                .Join(_context.VMPrice,
                prda => prda.Articulo.Id_Articulo,
                p => p.Id_Articulo,
                (prda, p) => new { PromoDA = prda, Precio = p }
                )
                .Where(pedp => pedp.PromoDA.PromoED.PromoE.FechaInicio <= DateTime.Now
                    && pedp.PromoDA.PromoED.PromoE.FechaFin >= DateTime.Now
                    && pedp.PromoDA.PromoED.PromoE.Id_Tipo == 2
                    && pedp.PromoDA.PromoED.PromoE.ListPromoDescuento_D.Count > 0
                )
                .Select(pedp => new
                {
                    Value = pedp.PromoDA.PromoED.PromoE.Id_Promocion.ToString(),
                    pedp.PromoDA.PromoED.PromoE.Nombre,
                    Porcentaje = pedp.PromoDA.PromoED.PromoE.Porcentaje,
                    pedp.PromoDA.PromoED.PromoE.FechaInicio,
                    pedp.PromoDA.PromoED.PromoE.FechaFin,
                    pedp.PromoDA.PromoED.PromoE.Id_Tipo,
                    pedp.PromoDA.PromoED.PromoE.ListPromoDescuento_D,
                    ValuePromoD = pedp.PromoDA.PromoED.PromoD.Id_Articulo,
                    TextPromoD = pedp.PromoDA.Articulo.Nombre,
                    Precio = pedp.PromoDA.Articulo.Precio.Precio,
                    PrecioArticulo = calcularDescuento(pedp.PromoDA.Articulo.Precio.Precio, pedp.PromoDA.PromoED.PromoE.Porcentaje)
                })
                .ToListAsync();
            var jsonListPromociones = JsonConvert.SerializeObject(listaPromociones);
            ViewData["JsonListaPromociones"] = jsonListPromociones;

            // Lógica para obtener las promociones agrupadas:
            var listaPromocionesAgrupadas = await _context.VMPromoDescuento_E
                .Where(p => p.FechaInicio <= DateTime.Now
                    && p.FechaFin >= DateTime.Now
                    && p.Id_Tipo == 2)
                .Include(p => p.ListPromoDescuento_D)
                .Include(p => p.TipoPromoDescuento)
                .GroupBy(gbp => new { gbp.Id_Promocion, gbp.Nombre, gbp.Porcentaje })
                .Select(pd => new
                {
                    Value = pd.Key.Id_Promocion.ToString(),
                    pd.Key.Nombre,
                    Porcentaje = pd.Key.Porcentaje
                })
                .ToListAsync();
            var jsonListaPromocionesAgrupadas = JsonConvert.SerializeObject(listaPromocionesAgrupadas);
            ViewData["JsonListaPromocionesAgrupadas"] = jsonListaPromocionesAgrupadas;

            // Lógica para obtener la lista de Combos:
            var listaCombos = await _context.VMCombo
                .Select(c => new
                {
                    Value = c.Id_Combo.ToString(),
                    ArtPrincipal = c.Id_Articulo.ToString(),
                    ArtAgregado = c.Id_ArticuloAgregado.ToString(),
                })
                .ToListAsync();
            var jsonListCombos = JsonConvert.SerializeObject(listaCombos);
            ViewData["JsonListaCombos"] = jsonListCombos;

            // Lógica para traer artículos con combos:
            var listaArticulosConCombo = await _context.VMArticle
            .Include(a => a.Precio)
            .Include(a => a.Rubro)
            .Include(a => a.Combos)
            .Join(_context.VMCombo,
                a => a.Id_Articulo,
                c => c.Id_Articulo,
                (a, c) => new { Articulo = a, Combo = c })
            .Join(_context.VMArticle,
                ac => ac.Combo.Id_ArticuloAgregado,
                aa => aa.Id_Articulo,
                (ac, aa) => new { ArticuloCombo = ac, ArticuloAgregado = aa })
            .Join(_context.VMPrice,
                acaa => acaa.ArticuloAgregado.Id_Articulo,
                p => p.Id_Articulo,
                (acaa, p) => new { ArticuloComboAgregado = acaa, Precio = p })
            .Where(acap => acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Activo == true
                    && acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Precio != null
                    && acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Precio.Precio > 0
                    && acap.ArticuloComboAgregado.ArticuloCombo.Articulo.UsaCombo == true)
            .Select(acap => new
            {
                Value = acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Id_Articulo.ToString(), // Id Artículo principal.
                Text = acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Nombre, // Nombre Artículo principal.
                Precio = acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Precio.Precio, // Precio Artículo principal.
                Rubro = acap.ArticuloComboAgregado.ArticuloCombo.Articulo.Id_Rubro.ToString(), // Rubro Artículo principal.
                ValueAgregado = acap.ArticuloComboAgregado.ArticuloAgregado.Id_Articulo.ToString(), // Id Artículo agregado.
                ComboId = acap.ArticuloComboAgregado.ArticuloCombo.Combo.Id_Combo.ToString(), // Id Combo.
                PrecioAgregado = acap.Precio.Precio, // Precio Artículo agregado.
                TextAgregado = acap.ArticuloComboAgregado.ArticuloAgregado.Nombre // Nombre Artículo agregado.
            })
            // Ordernar por id de artículo principal.
            .OrderBy(acap => acap.ValueAgregado)
            .ToListAsync();

            var jsonListArticulosConCombo = JsonConvert.SerializeObject(listaArticulosConCombo);
            ViewData["JsonListaArticulosConCombo"] = jsonListArticulosConCombo;

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

            // Lógica para obtener los tipos de tarjeta:
            var listaTarjetas = await 
                (from tt in _context.VMTipoTarjeta
                 join tp in _context.VMTipoTransaccion
                 on tt.CodTipoTran equals tp.CodTipoTran into temp
                 from tp in temp.DefaultIfEmpty()
                 select new { tt.CodTarjeta, tp.Nombre })
                .ToListAsync();
            ViewData["ListaTarjetas"] = new SelectList(listaTarjetas, "CodTarjeta", "Nombre");
            
            // Lógica para obtener los tipos de trasacciones:
            var listaTipoTransacciones = await _context.VMTipoTransaccion
                .ToListAsync();
            ViewData["ListaTipoTransacciones"] = new SelectList(listaTipoTransacciones, "CodTipoTran", "Nombre");

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
        public void ReducirStock(int idArticulo, int cantidad)
        {
            var articulo = _context.VMArticle
                .Where(a => a.Id_Articulo == idArticulo)
                .Include(a => a.Stock)
                .FirstOrDefault();

            if (articulo.UsaStock && articulo.Stock.Cantidad > 0)
            {
                articulo.Stock.Cantidad -= cantidad;

                if(articulo.Stock.Cantidad < 0)
                {
                    articulo.Stock.Cantidad = 0;
                }

                _context.SaveChanges();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta(VMVentas vMVentas, 
                                                    List<VMVentasDetalle> detallesventa, 
                                                    VMVentaImporte vMVentaImporte,
                                                    List<VMFormaPagoSelect> formaPagoSelects)
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

            decimal precioU = 0;

            int renglon = 1; // Inicialización de Renglon:

            // Datos Ventas I:
            decimal impSubTotal = 0;

            var descuentoEntity = await _context.VMPromoDescuento_E
                .Where(p => p.Id_Promocion == (vMVentaImporte.Descuento != null ? int.Parse(vMVentaImporte.Descuento) : 0))
                .FirstOrDefaultAsync();

            decimal PorcentajeDescuento = 0;

            if (descuentoEntity != null)
            {
                PorcentajeDescuento = descuentoEntity.Porcentaje;
            }
            else
            {
                PorcentajeDescuento = 0;
            }

            var conceptos = await _context.VMConcepto
                .ToListAsync();

            decimal iva21 = 1.21M;

            // Datos Ventas_TipoTransacción:
            decimal importeTotal = 0;

            // Traer el tipo de transacción:
            if(vMVentas.FormaPago.Nombre == "Tarjeta")
            {
                vMVentas.VMTipoTransaccion = await _context.VMTipoTransaccion
                .Where(tt => tt.CodTipoTran == vMVentas.CodTarjeta)
                .FirstOrDefaultAsync();
            }
            else
            {
                vMVentas.VMTipoTransaccion = await _context.VMTipoTransaccion
                .Where(tt => tt.Nombre == vMVentas.FormaPago.Nombre)
                .FirstOrDefaultAsync();
            }

            // Si el tipo de transacción no existe, lo crea y luego lo asigna:
            if(vMVentas.VMTipoTransaccion  == null)
            {
                string codTipoTranVar = vMVentas.FormaPago.Nombre.Replace(" ", "").ToUpper();


                var tipoTransaccion = new VMTipoTransaccion
                {
                    CodTipoTran = codTipoTranVar,
                    Nombre = vMVentas.FormaPago.Nombre
                };
                _context.VMTipoTransaccion.Add(tipoTransaccion);

                await _context.SaveChangesAsync();

                vMVentas.VMTipoTransaccion = await _context.VMTipoTransaccion
                .Where(tt => tt.CodTipoTran == codTipoTranVar)
                .FirstOrDefaultAsync();
            }

            // Cambiar . por ,:
            string pagoEfectivo = vMVentas.Pago.Replace('.',',');
            string vueltoEfectivo = vMVentas.Vuelto.Replace('.', ',');

            // Obtener el número de venta:
            decimal nroVenta = await ObtenerNumVenta(vMVentas.Sucursal.NumSucursal,
                vMVentas.Comprobante_E.CodComprobante,
                vMVentas.Modulo.CodModulo);

            // Variables promociones:
            var promocion = new VMPromoDescuento_E();


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
                            .Include(a => a.Precio)
                            .FirstOrDefaultAsync();

                        if (detalle.AplicaPromo == "1")
                        {
                            promocion = await _context.VMPromoDescuento_E
                                .Where(p => p.Id_Promocion == int.Parse(detalle.Id_Promo))
                                .FirstOrDefaultAsync();

                            precioU = calcularDescuento(articulo.Precio.Precio, promocion.Porcentaje);
                        }
                        else
                        {
                            precioU = articulo.Precio.Precio;
                        }

                        if(articulo.UsaStock)
                        {
                            ReducirStock(articulo.Id_Articulo, int.Parse(detalle.Cantidad));
                        }

                        var ventaD = new VMVentas_D
                        {
                            NumVenta = nroVentaCorrelativa, // Se obtiene con la función de obtener número de venta. OK
                            CodComprobante = vMVentas.Comprobante_E.CodComprobante, // Se obtiene de la vista. OK
                            CodModulo = vMVentas.Modulo.CodModulo, // Se obtiene de la vista (HardCodeado a VTAS). OK
                            NumSucursal = vMVentas.Sucursal.NumSucursal, // Se obtiene de la base (Tabla Config). OK
                            Renglon = renglon, // Se incrementa en cada iteración. OK
                            Id_Articulo = int.Parse(detalle.Id_Articulo), // Se obtiene de la vista. OK
                            Cantidad = int.Parse(detalle.Cantidad), // Se obtiene de la vista. OK
                            Detalle = detalle.Detalle, // Se obtiene de la base de datos. OK
                            PrecioUnitario = precioU, // Se obtiene de la base de datos. OK
                            PrecioTotal = int.Parse(detalle.Cantidad) * precioU // Se calcula acá. OK
                        };
                        _context.VMVentas_D.Add(ventaD);

                        // Se incrementa el renglón:
                        renglon++;

                        // Se calcula el importe subtotal:
                        impSubTotal += ventaD.PrecioTotal; // El importa 
                    }

                    // Se da de alta el importe de la venta:
                    foreach (var concepto in conceptos)
                    {
                        decimal descuento = (impSubTotal * PorcentajeDescuento) / 100;

                        if (descuento == 0 && concepto.CodConcepto == "DTO")
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
                                importeTotal = importe;
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

                    // Se da de alta el tipo de transacción (Ventas_TipoTransaccion):

                    if (vMVentas.VMTipoTransaccion.CodTipoTran == "EFECTIVO" && vMVentas.FormaPago.Nombre == "Efectivo")
                    {
                        List<string> tipoTran = new List<string> { "EFECTIVO", "VUELTO" };

                        int i = 1;
                        foreach ( var tipo in tipoTran )
                        {

                            if (tipo == "EFECTIVO")
                                importeTotal = decimal.Parse(pagoEfectivo);
                            else
                                importeTotal = decimal.Parse(vueltoEfectivo);

                            var ventasTipoTransaccion = new VMVentas_TipoTransaccion
                            {
                                CodTipoTran = tipo,
                                NumTransaccion = $"{nroVentaCorrelativa}/{i}",
                                NumVenta = nroVentaCorrelativa,
                                CodComprobante = vMVentas.Comprobante_E.CodComprobante,
                                CodModulo = vMVentas.Modulo.CodModulo,
                                NumSucursal = vMVentas.Sucursal.NumSucursal,
                                Importe = importeTotal
                            };

                            _context.VMVentas_TipoTransaccion.Add(ventasTipoTransaccion);

                            i++;
                        }
                    }
                    else if (vMVentas.FormaPago.Nombre == "Varias")
                    {
                        int i = 1;

                        var monto = "";

                        foreach (var forma in formaPagoSelects)
                        {
                            monto = forma.Monto.Replace('.', ',');

                            string codTipoTransaccion = "";

                            var codTran = await _context.VMTipoTransaccion
                                .Join(_context.VMFormaPago,
                                    t => t.Nombre,
                                    f => f.Nombre,
                                    (t, f) => new { TipoTransaccion = t, FormaPago = f })
                                .Where(tf => tf.FormaPago.Id_FormaPago == forma.Id)
                                .Select(tf => new { tf.TipoTransaccion.CodTipoTran })
                                .FirstOrDefaultAsync();

                            var formaPagoSeleccionada = await _context.VMFormaPago
                                .Where(f => f.Id_FormaPago == forma.Id)
                                .FirstOrDefaultAsync();

                            if (formaPagoSeleccionada.Nombre == "Tarjeta")
                            {
                                codTipoTransaccion = vMVentas.CodTarjeta;
                            }
                            else
                            {
                                codTipoTransaccion = codTran.CodTipoTran;
                            }

                            importeTotal = decimal.Parse(monto);

                            var ventasTipoTransaccion = new VMVentas_TipoTransaccion
                            {
                                CodTipoTran = codTipoTransaccion,
                                NumTransaccion = $"{nroVentaCorrelativa}/{i}",
                                NumVenta = nroVentaCorrelativa,
                                CodComprobante = vMVentas.Comprobante_E.CodComprobante,
                                CodModulo = vMVentas.Modulo.CodModulo,
                                NumSucursal = vMVentas.Sucursal.NumSucursal,
                                Importe = importeTotal
                            };
                            _context.VMVentas_TipoTransaccion.Add(ventasTipoTransaccion);

                            i++;
                        }
                    }
                    else
                    {
                        int i = 1;

                        var ventasTipoTransaccion = new VMVentas_TipoTransaccion
                        {
                            CodTipoTran = vMVentas.VMTipoTransaccion.CodTipoTran,
                            NumTransaccion = $"{nroVentaCorrelativa}/{i}",
                            NumVenta = nroVentaCorrelativa,
                            CodComprobante = vMVentas.Comprobante_E.CodComprobante,
                            CodModulo = vMVentas.Modulo.CodModulo,
                            NumSucursal = vMVentas.Sucursal.NumSucursal,
                            Importe = importeTotal
                        };
                        _context.VMVentas_TipoTransaccion.Add(ventasTipoTransaccion);
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

                    if (comprobanteN != null)
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

                    return Json(new { success = true, 
                        message = "\nSe insertó la venta nro: " + nroVentaCorrelativa + " correctamente. \nDetalle de la venta: " + (detallesventa.Count - 1) + " artículos."
                    });
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
            printer.agregarLinea("Sucursal: " + vMVentas_E.Sucursal.NumSucursal, 7, "Consolas");
            printer.agregarLinea("Número de Venta: " + vMVentas_E.NumVenta, 7, "Consolas");
            printer.agregarLinea("Fecha: " + vMVentas_E.Fecha.ToString("dd/MM/yyyy"), 7, "Consolas");
            printer.agregarLinea("Hora: " + vMVentas_E.Hora.ToString("HH:mm:ss"), 7, "Consolas");
            printer.agregarLinea("Cliente: " + vMVentas_E.Cliente.RazonSocial, 7, "Consolas");
            printer.agregarLinea("Forma De Pago: " + vMVentas_E.FormaPago.Nombre, 7, "Consolas");

            printer.agregarLineaEnBlanco();

            printer.agregarLinea("Detalle de la venta:", 7, "Consolas", FontStyle.Bold);

            foreach(var detalle in vMVentas_D)
            {
                printer.agregarLineaConExtremos(detalle.Detalle, detalle.Cantidad.ToString() + " x " + detalle.PrecioUnitario.ToString("0.00") + " = " + detalle.PrecioTotal.ToString("0.00"), 7, "Consolas");
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

        public static decimal calcularDescuento(decimal precio, decimal porcentaje) => (precio - ((precio * porcentaje) / 100));
    }
}