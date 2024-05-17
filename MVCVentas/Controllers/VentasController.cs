using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
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
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using Font = iTextSharp.text.Font;

namespace MVCVentas.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly MVCVentasContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public VentasController(MVCVentasContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        string rutaRaizApp = AppDomain.CurrentDomain.BaseDirectory;

        #region Traer datos al Sistema de Ventas (Index):

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

            // Lógica para obtener las formas de pago sin varios
            var listaMedioPagoModal = await _context.VMFormaPago
                .Where(f => f.Nombre != "Varias")
                .ToListAsync();
            ViewData["ListaMedioPagoModal"] = new SelectList(listaMedioPagoModal, "Id_FormaPago", "Nombre");

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

        #endregion

        #region Metodo: CrearCliente

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

        #endregion

        #region ComprobantesN y Número de Venta

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
        public async Task<string> ObtenerNumVenta(string numSucursal, string codComprobante, string codModulo)
        {
            decimal nroVenta = 0;

            var comprobante = await _context.VMComprobante_N
                .Where(c =>
                    c.NumSucursal == numSucursal
                    && c.CodComprobante == codComprobante
                    && c.CodModulo == codModulo)
                .FirstOrDefaultAsync();

            if (comprobante == null)
            {
                var altaComprobanteResponse = await AltaComprobanteN(new VMComprobante_N
                {
                    NumSucursal = numSucursal,
                    CodComprobante = codComprobante,
                    CodModulo = codModulo
                }) as JsonResult;

                if (altaComprobanteResponse != null)
                {
                    dynamic altaComprobanteData = altaComprobanteResponse.Value;

                    if (altaComprobanteData.success == true)
                    {
                        return "00000001";
                    }
                }

            }

            nroVenta = comprobante.NumComprobante;

            // Se incrementa el número de venta:
            decimal nroVentaDecimal = nroVenta + 1;

            // Se convierte el número de venta a string y se le agrega ceros a la izquierda:
            string nroVentaString = nroVentaDecimal.ToString();
            string nroVentaCorrelativa = nroVentaString.PadLeft(8, '0');

            return nroVentaCorrelativa;
        }

        #endregion

        #region Stock

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

        #endregion

        #region Alta de Venta

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
            if (vMVentas.FormaPago.Nombre == "Tarjeta")
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

            // Datos de PedidosActuales:

            int renglonPedido = 1;

            // Obtener número de pedido correlativo:
            var ultPedido = await _context.vMPedidosActuales
                .OrderByDescending(p => p.FechaCreacion)
                .FirstOrDefaultAsync();

            var nroPedido = 0;

            if (ultPedido != null) {
                nroPedido = ultPedido.NumPedido + 1;
            }

            if (vMVentas.Retira is null) vMVentas.Retira = "Sin Nombre";

            // Cambiar . por ,:
            string pagoEfectivo = vMVentas.Pago.Replace('.',',');
            string vueltoEfectivo = vMVentas.Vuelto.Replace('.', ',');

            // Obtener el número de venta:
            string nroVentaCorrelativa = await ObtenerNumVenta(vMVentas.Sucursal.NumSucursal,
                vMVentas.Comprobante_E.CodComprobante,
                vMVentas.Modulo.CodModulo);

            // Variables promociones:
            var promocion = new VMPromoDescuento_E();

            // Lista única para cupones:
            HashSet<string> cuponesUnicos = new HashSet<string>();

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
                        else if (detalle.EsCupon == "1")
                        {
                            precioU = calcularDescuento(articulo.Precio.Precio, decimal.Parse(detalle.PorcentajeDescuentoCupon));

                            cuponesUnicos.Add(detalle.NroCupon);
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
                        impSubTotal += ventaD.PrecioTotal; // El importe 
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

                    // Se da de alta el pedido en PedidosActuales:
                    foreach ( var detalle in detallesventa)
                    {
                        if (detalle.Id_Articulo is null) continue;

                        var pedidoActual = new VMPedidoActual
                        {
                            NumPedido = nroPedido,
                            NumVenta = nroVentaCorrelativa,
                            CodComprobante = vMVentas.Comprobante_E.CodComprobante,
                            CodModulo = vMVentas.Modulo.CodModulo,
                            NumSucursal = vMVentas.Sucursal.NumSucursal,
                            Renglon = renglonPedido,
                            Id_Articulo = int.Parse(detalle.Id_Articulo),
                            Cantidad = int.Parse(detalle.Cantidad),
                            Retira = vMVentas.Retira,
                            FechaCreacion = DateTime.Now,
                            FechaExpiracion = DateTime.Now.AddMinutes(15)
                        };
                        _context.vMPedidosActuales.Add(pedidoActual);
                        renglonPedido++;
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
                        comprobanteN.NumComprobante = comprobanteN.NumComprobante + 1;
                        await _context.SaveChangesAsync();
                    }

                    // Proceso de quemado de Cupones
                    // QuemarCupon(string nroCupon)
                    foreach (var numeroCupon in cuponesUnicos)
                    {
                        await QuemarCupon(numeroCupon);
                    }

                    // Se confirma la transacción:
                    transaction.Commit();

                    // Traigo una lista de los registros de Ventas_D filtrando por claves primarias (la venta realizada)
                    var listVentaD = await _context.VMVentas_D
                        .Where(v => v.NumVenta == nroVentaCorrelativa
                                && v.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                                && v.CodModulo == vMVentas.Modulo.CodModulo
                                && v.NumSucursal == vMVentas.Sucursal.NumSucursal)
                        .ToListAsync();

                    // Traigo una lista de los registros de Ventas_I filtrando por claves primarias (la venta realizada)
                    var listVentaI = await _context.VMVentas_I
                        .Where(v => v.NumVenta == nroVentaCorrelativa
                                && v.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                                && v.CodModulo == vMVentas.Modulo.CodModulo
                                && v.NumSucursal == vMVentas.Sucursal.NumSucursal)
                        .ToListAsync();

                    // Traigo una lista de los nombres de tipo de transacción filtrando por la venta realizada
                    var listVentaTipoTransaccion = await _context.VMVentas_TipoTransaccion
                        .Join(_context.VMTipoTransaccion,
                            vt => vt.CodTipoTran,
                            tt => tt.CodTipoTran,
                            (vt, tt) => new { Ventas_TipoTransaccion = vt, TipoTransaccion = tt })
                        .Where(v => v.Ventas_TipoTransaccion.NumVenta == nroVentaCorrelativa
                                && v.Ventas_TipoTransaccion.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                                && v.Ventas_TipoTransaccion.CodModulo == vMVentas.Modulo.CodModulo
                                && v.Ventas_TipoTransaccion.NumSucursal == vMVentas.Sucursal.NumSucursal)
                        .Select( v => v.TipoTransaccion.Nombre)
                        .ToListAsync();

                    // Traigo Ventas_E
                    var ventas_E = await _context.VMVentas_E
                    .Include(v => v.Cliente)
                        .ThenInclude(c => c.Provincia)
                    .Include(v => v.Cliente)
                        .ThenInclude(c => c.Localidad)
                    .Include(v => v.Usuario)
                        .ThenInclude(u => u.Categoria)
                    .Include(v => v.Ventas_D)
                        .ThenInclude(vd => vd.Articulo)
                            .ThenInclude(vda => vda.Rubro)
                    .Where(v => v.NumVenta == nroVentaCorrelativa
                        && v.CodComprobante == vMVentas.Comprobante_E.CodComprobante
                        && v.CodModulo == vMVentas.Modulo.CodModulo
                        && v.NumSucursal == vMVentas.Sucursal.NumSucursal)
                    .FirstOrDefaultAsync();

                    var responseGenerarReporte = await GenerarReportePDF(ventaE, listVentaD, listVentaI, listVentaTipoTransaccion, rutaRaizApp);
                    ImprimirReporte(ventaE, listVentaD, listVentaI, listVentaTipoTransaccion);

                    //var resultEnvioVenta = await EnviarVenta(ventas_E);

                    return Json(new
                    {
                        success = true,
                        message = "\nSe insertó la venta nro: " + nroVentaCorrelativa + " correctamente. \nDetalle de la venta: " +
                        (detallesventa.Count - 1) + " artículos.",
                        messageGenerarReporte = responseGenerarReporte
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        #endregion

        #region Eliminar Venta

        public async Task<IActionResult> EliminarVenta(string numVenta, string codComprobante, string codModulo, string numSucursal)
        {
            // Verificar que los parametros no sean nulos
            if( string.IsNullOrEmpty(numVenta) || string.IsNullOrEmpty(codComprobante) || string.IsNullOrEmpty(codModulo) || string.IsNullOrEmpty(numSucursal) )
                return NotFound();

            // Obtener las ventas:
            var vMVentas_E = await _context.VMVentas_E.FindAsync(numVenta, codComprobante, codModulo, numSucursal);
            if(vMVentas_E == null)
                return NotFound("Venta no encontrada");

            var vMVentas_D = await _context.VMVentas_D
               .Where(v => v.NumVenta == numVenta
                   && v.CodComprobante == codComprobante
                   && v.CodModulo == codModulo
                   && v.NumSucursal == numSucursal)
               .ToListAsync();
            if (vMVentas_D == null)
                return NotFound("Venta no encontrada");

            var vMVentas_I = await _context.VMVentas_I
                .Where(v => v.NumVenta == numVenta
                    && v.CodComprobante == codComprobante
                    && v.CodModulo == codModulo
                    && v.NumSucursal == numSucursal)
               .ToListAsync();
            if (vMVentas_I == null)
                return NotFound("Venta no encontrada");

            var vMVentas_TipoTransaccion = await _context.VMVentas_TipoTransaccion
                .Where(v => v.NumVenta == numVenta 
                    && v.CodComprobante == codComprobante 
                    && v.CodModulo == codModulo 
                    && v.NumSucursal == numSucursal)
                .ToListAsync();
            if (vMVentas_TipoTransaccion == null)
                return NotFound("Venta no encontrada");

            try
            {
                // Eliminar los registros:

                foreach(var ventas_tipoTransaccion in vMVentas_TipoTransaccion)
                {
                    _context.VMVentas_TipoTransaccion.Remove(ventas_tipoTransaccion);
                }

                foreach(var ventas_I in vMVentas_I)
                {
                    _context.VMVentas_I.Remove(ventas_I);
                }

                foreach (var ventas_D in vMVentas_D)
                {
                    _context.VMVentas_D.Remove(ventas_D);
                }

                _context.VMVentas_E.Remove(vMVentas_E);

                /////////////// Está pendiente la corrección de la numeración ///////////////

                // Corregir numeración de comprobante:
                //var comprobanteN = await _context.VMComprobante_N.FindAsync(codComprobante, codModulo, numSucursal);
                //var numVentaEliminada = vMVentas_E.NumVenta.TrimStart('0');

                //comprobanteN.NumComprobante = int.Parse(numVentaEliminada) - 1;
                //_context.VMComprobante_N.Update(comprobanteN);

                /////////////// Está pendiente la corrección de la numeración ///////////////

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al eliminar la venta, error:" + ex.Message);
            }

            return Ok(new { message = "Venta eliminada correctamente" });
        }

        #endregion

        #region Generar Reporte

        public Printer GenerarReporte(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I, List<string> vMVentas_TipoTransaccion)
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

            // Si la forma de pago es Tarjetas, imprime el nombre de la tarjeta
            if(vMVentas_E.FormaPago.Nombre == "Tarjeta")
                printer.agregarLinea("Forma De Pago: " + vMVentas_TipoTransaccion[0], 7, "Consolas");
            else
            printer.agregarLinea("Forma De Pago: " + vMVentas_E.FormaPago.Nombre, 7, "Consolas");

            // Si la forma de pago es "varias", se imprimirá además de "Varias", las formas de pago utilizadas.
            if (vMVentas_E.FormaPago.Nombre == "Varias")
            {
                foreach (var ventaTipoTran in vMVentas_TipoTransaccion)
                {
                    printer.agregarLinea("---> " + ventaTipoTran, 7, "Consolas");
                }
            }

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

        public void ImprimirReporte(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I, List<string> vMVentas_TipoTransaccion)
        {
            Printer printer = GenerarReporte(vMVentas_E, vMVentas_D, vMVentas_I, vMVentas_TipoTransaccion);

            printer.Imprimir();
        }

        public async Task<string> GenerarReportePDF(VMVentas_E vMVentas_E, List<VMVentas_D> vMVentas_D, List<VMVentas_I> vMVentas_I, List<string> vMVentas_TipoTransaccion, string projectPath)
        {
            string rutaComprobantesImpresos = Path.Combine(projectPath, "Comprobantes Impresos");
            string rutaCodComprobate = Path.Combine(rutaComprobantesImpresos, vMVentas_E.CodComprobante);
            string rutaNumSucursal = Path.Combine(rutaCodComprobate, vMVentas_E.NumSucursal);
            string rutaReporte = Path.Combine(rutaNumSucursal, vMVentas_E.NumVenta + ".pdf");

            Directory.CreateDirectory(rutaComprobantesImpresos);
            Directory.CreateDirectory(rutaCodComprobate);
            Directory.CreateDirectory(rutaNumSucursal);

            Printer printer = GenerarReporte(vMVentas_E, vMVentas_D, vMVentas_I, vMVentas_TipoTransaccion);

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

            var response = await EnviarEmailConFactura(vMVentas_E.Cliente.Email, vMVentas_E.Cliente.RazonSocial, "Se adjunta factura. Gracias por su compra.", rutaReporte);

            return response;
        }

        public async Task<string> EnviarEmailConFactura(string emailTo, string client, string emailBody, string projectPath)
        {
            byte[] pdfBytes = System.IO.File.ReadAllBytes(projectPath);
            string responseData;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var ventasApiClient = _httpClientFactory.CreateClient("VentasApiClient");
                ventasApiClient.DefaultRequestHeaders.Accept.Clear();
                ventasApiClient.DefaultRequestHeaders.Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                using (var formData = new MultipartFormDataContent())
                {
                    // Adjuntar archivo PDF.
                    formData.Add(new ByteArrayContent(pdfBytes, 0, pdfBytes.Length), "file", "factura.pdf");

                    // Datos para el correo.
                    formData.Add(new StringContent(emailTo), "EmailTo");
                    formData.Add(new StringContent(client), "client");
                    formData.Add(new StringContent(emailBody), "emailBody");

                    var response = await ventasApiClient.PostAsync("/mail/sendmail", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        return "Email con factura envíada correctamente";
                    }

                    else
                    {
                        return "Error al enviar email con la factura: " + response.ReasonPhrase;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error al enviar email con la factura: " + ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                Log.Information($"EnviarEmailConFactura(...)\nTiempo de ejecución: {stopwatch.ElapsedMilliseconds} ms, {(decimal)stopwatch.ElapsedMilliseconds / 1000} segs");
            }
        }

        #endregion

        #region Descuento

        public static decimal calcularDescuento(decimal precio, decimal porcentaje) => (precio - ((precio * porcentaje) / 100));

        #endregion

        #region Enviar Venta a API

        public async Task<string> EnviarVenta(VMVentas_E ventas_E)
        {
            dynamic jsonData = new ExpandoObject();

            // Asignar valores de VMVentas
            jsonData.numVenta = ventas_E.NumVenta;
            jsonData.codComprobante = ventas_E.CodComprobante;
            jsonData.codModulo = ventas_E.CodModulo;
            jsonData.numSucursal = ventas_E.NumSucursal;
            jsonData.fecha = ventas_E.Fecha;
            jsonData.hora = ventas_E.Hora;
            jsonData.formaDePago = new
            {
                id_FormaPago = ventas_E.FormaPago.Id_FormaPago,
                nombre = ventas_E.FormaPago.Nombre
            };
            jsonData.cliente = new
            {
                codCliente = ventas_E.Cliente.CodCliente,
                cuit = ventas_E.Cliente.CUIT,
                razonSocial = ventas_E.Cliente.RazonSocial,
                nombre = ventas_E.Cliente.Nombre,
                telefono = ventas_E.Cliente.Telefono,
                email = ventas_E.Cliente.Email,
                direccion = ventas_E.Cliente.Direccion,
                provincia = new
                {
                    codProvincia = ventas_E.Cliente.Provincia.CodProvincia,
                    nombre = ventas_E.Cliente.Provincia.Nombre
                },
                localidad = new
                {
                    codLocalidad = ventas_E.Cliente.Localidad.CodLocalidad,
                    nombre = ventas_E.Cliente.Localidad.Nombre
                },
                fechaAlta = ventas_E.Cliente.FechaAlta
            };
            jsonData.usuario = new
            {
                id_Usuario = ventas_E.Usuario.Id_Usuario,
                categoria = new
                {
                    id_Categoria = ventas_E.Usuario.Categoria.Id_Categoria,
                    nombre = ventas_E.Usuario.Categoria.Nombre
                },
                usuario = ventas_E.Usuario.Usuario,
                password = ventas_E.Usuario.Password,
                estado = ventas_E.Usuario.Estado,
                fecha = ventas_E.Usuario.Fecha,
                nombre = ventas_E.Usuario.Nombre,
                apellido = ventas_E.Usuario.Apellido
            };
            jsonData.numcaja = ventas_E.NumCaja;

            jsonData.ventaDetalle = new List<dynamic>();
            foreach(var ventas_D in ventas_E.Ventas_D)
            {

                dynamic detalleJson = new ExpandoObject();
                detalleJson.renglon = ventas_D.Renglon;
                detalleJson.articulo = new
                {
                    id_Articulo = ventas_D.Id_Articulo,
                    nombre = ventas_D.Articulo.Nombre,
                    rubro = new
                    {
                        id_Rubro = ventas_D.Articulo.Rubro.Id_Rubro,
                        nombre = ventas_D.Articulo.Rubro.Nombre
                    },
                    activo = ventas_D.Articulo.Activo,
                    descripcion = ventas_D.Articulo.Descripcion,
                    fecha = ventas_D.Articulo.Fecha,
                    usaStock = ventas_D.Articulo.UsaStock,
                    usaCombo = ventas_D.Articulo.UsaCombo
                };
                detalleJson.cantidad = ventas_D.Cantidad;
                detalleJson.detalle = ventas_D.Detalle;
                detalleJson.precioUnitario = Math.Round(ventas_D.PrecioUnitario, 2);
                detalleJson.precioTotal = Math.Round(ventas_D.PrecioTotal, 2);

                jsonData.ventaDetalle.Add(detalleJson);
            }

            jsonData.ventaImporte = new List<dynamic>();
            foreach(var ventas_I in ventas_E.Ventas_I)
            {
                dynamic importeJson = new ExpandoObject();
                importeJson.concepto = new
                {
                    codConcepto = ventas_I.Concepto.CodConcepto,
                    descripcion = ventas_I.Concepto.Descripcion,
                    porcentaje = ventas_I.Concepto.Porcentaje
                };
                importeJson.importe = Math.Round(ventas_I.Importe, 2);
                importeJson.descuento = ventas_I.Descuento;

                jsonData.ventaImporte.Add(importeJson);
            }

            string responseData;
            try
            {
                var ventasApiClient = _httpClientFactory.CreateClient("VentasApiClient");

                ventasApiClient.DefaultRequestHeaders.Accept.Clear();
                ventasApiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(jsonData);
                System.IO.File.WriteAllText(@"C:\Repositorio\archivo.json", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ventasApiClient.PostAsync("api/ventas", content);

                if (response.IsSuccessStatusCode)
                {
                    responseData = "Venta enviada correctamente. Respuesta de la API: " + response.StatusCode;
                }
                else
                {
                    responseData = "Error al enviar la venta. Detalles: " + response.ReasonPhrase;
                }

                return responseData.ToString();
            }
            catch (Exception ex)
            {
                responseData = "Error al enviar la venta. Detalles: " + ex.Message.ToString();
                return responseData;
            }

        }

        #endregion

        #region Cupones

        public async Task<IActionResult> QuemarCupon(string nroCupon)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var wsCuponesClient = _httpClientFactory.CreateClient("WSCuponesClient");
                wsCuponesClient.DefaultRequestHeaders.Accept.Clear();
                wsCuponesClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                    .MediaTypeWithQualityHeaderValue("application/json"));

                var nroCuponJson = JsonConvert.SerializeObject(nroCupon);

                var content = new StringContent(nroCuponJson, Encoding.UTF8, "application/json");

                var response = await wsCuponesClient.PostAsync($"api/Cupones/QuemarCupon", content);

                if (response.IsSuccessStatusCode)
                {
                    var cuponJson = await response.Content.ReadAsStringAsync();

                    return Json(new { success = true, message = $"Cupón {nroCupon} utilizado." });
                }
                else
                {
                    throw new Exception("Error al quemar el cupón. Detalles: " + response.ReasonPhrase + "\nMetodo: QuemarCupon()");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error interno del servidor: " + ex.Message + "\nMetodo: QuemarCupon()");
            }
            finally
            {
                stopwatch.Stop();
                Log.Information($"QuemarCupon(...)\nTiempo de ejecución: {stopwatch.ElapsedMilliseconds} ms, {(decimal)stopwatch.ElapsedMilliseconds / 1000} segs");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCupon(string nroCupon)
        {
            string errorMessage;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var wsCuponesClient = _httpClientFactory.CreateClient("WSCuponesClient");
                wsCuponesClient.DefaultRequestHeaders.Accept.Clear();
                wsCuponesClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                    .MediaTypeWithQualityHeaderValue("application/json"));

                var response = await wsCuponesClient.GetAsync($"api/Cupones/Cupon/{nroCupon}");

                if (response.IsSuccessStatusCode)
                {
                    var cuponJson = await response.Content.ReadAsStringAsync();

                    //Deserializar Json:
                    var vMCupon = JsonConvert.DeserializeObject<VMCupon>(cuponJson);

                    var articulosCuponResponse = await ObtenerArticulosDeCupon(vMCupon, nroCupon);
                    var articulosCuponResult = articulosCuponResponse as OkObjectResult;
                    var articulosCupon = articulosCuponResult.Value;

                    return Json(new {success = true, articulosCupon});
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                    errorMessage = errorMessage.Replace("\"", "");
                    return Json(new {success = false, message = errorMessage});
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error, no se logró obtener los datos.";
                errorMessage = errorMessage.Replace("\"", "");
                return Json(new { success = false, message = errorMessage });
            }
            finally
            {
                stopwatch.Stop();
                Log.Information($"ValidarCupon(...)\nTiempo de ejecución: {stopwatch.ElapsedMilliseconds} ms, {(decimal)stopwatch.ElapsedMilliseconds / 1000} segs");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerArticulosDeCupon(VMCupon vMCupon, string nroCupon)
        {
            // Traer el id de los artículos de los cupones:
            List<int> idsArticulosAsociados = vMCupon.Detalle.Select(d => d.Id_ArticuloAsociado).ToList();

            // Guardo los artículos y cantidad en una nueva lista y luego lo guardo en un diccionario.
            var ArticulosYCant = vMCupon.Detalle
                .Select(g => new { id_Articulo = g.Id_ArticuloAsociado, Cantidad = g.Cantidad })
                .ToList();
            var cantidadesPorId = ArticulosYCant.ToDictionary(id => id.id_Articulo, id => id.Cantidad);

            // Traer los artículos por id y seleccionar campos
            var articulos = await _context.VMArticle
                .Include(a => a.Precio)
                .Where(a => idsArticulosAsociados.Any(id => id == a.Id_Articulo))
                .Select(ap => new
                {
                    NroCupon = nroCupon,
                    Id_Articulo = ap.Id_Articulo,
                    NombreArt = ap.Nombre,
                    Cantidad = cantidadesPorId.ContainsKey(ap.Id_Articulo) ? cantidadesPorId[ap.Id_Articulo] : 0,
                    Precio = Math.Round(calcularDescuento(ap.Precio.Precio, vMCupon.PorcentajeDto), 2),
                    PorcentajeDescuentoCupon = vMCupon.PorcentajeDto,
                    Total = Math.Round((cantidadesPorId.ContainsKey(ap.Id_Articulo) ? cantidadesPorId[ap.Id_Articulo] : 0) * (calcularDescuento(ap.Precio.Precio, vMCupon.PorcentajeDto)), 2)
                })
                .ToListAsync();

            Console.WriteLine(articulos);

            // Retornar JSON:
            return Ok(articulos);
        }

        #endregion

    }
}