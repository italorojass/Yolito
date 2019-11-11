using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;
using MvcSiteMapProvider;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.BusinessConnectorNet;

namespace PaginaDefinitivaYolito.Controllers
{
    public class CompraController : Controller
    {
        //
        // GET: /Compra/
        private PaginaWebEntities1 db = new PaginaWebEntities1();
        private Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [MvcBreadCrumbs.BreadCrumb]
        public ActionResult Paso1(int id)
        {
            var traecarro = from cr in db.CabeceraCarro
                            from cp in db.DetalleCarro
                            where cr.NPedido == id && cp.NPedido == id
                            select cr;
            if (Session["rutuser"] != null)
            {
                MvcBreadCrumbs.BreadCrumb.SetLabel("Despacho");
                return View("Despacho");
            }
            else
            {
                MvcBreadCrumbs.BreadCrumb.SetLabel("Inicio");
                return PartialView("Acceder",traecarro);
            }
        }

        [HttpGet]
        public ActionResult CarroDeCompras()
        {
            
            if (Session["cart"] != null)
            {
                var cart = Session["cart"].ToString();
                int elcart = Convert.ToInt32(cart);

                var carro = from cr in db.DetalleCarro
                            where cr.NPedido == elcart
                            select cr;

                
                return View("ElCarro", carro.ToList());

            }else{

                return View("ElCarro");
            }
            
       }

        
        public void AddCabecera()
        {
            // crear cabecera y asignarla al cliente
            CabeceraCarro creaCabecera = new CabeceraCarro();
            string ip = Request.UserHostName;
            
             //fecha de hoy
            DateTime hoydia = DateTime.Now;
            //string fecha = hoydia.ToString("MMdd");
            //string fecha_exito = hoydia.ToString("dd/MM/yyyy");
            
            if (!Request.Browser.Crawler )
            {
                creaCabecera.EstadoOc = false;
                creaCabecera.IPAdress = ip;
                creaCabecera.FechaCarro = hoydia;
                db.CabeceraCarro.Add(creaCabecera);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception op)
                {
                    Response.Write(op.Message);
                }
                //una vez insertado el npedido, insertarlo en clientes si esta autenticado
                var traeNpedido = (from cb in db.CabeceraCarro
                                   select cb);
                var ped = 0;
                foreach (var sess in traeNpedido)
                {
                    ped = sess.NPedido;
                }
                Session["cart"] = ped;
            }

            
        }

        [HttpPost]
        public ActionResult AgregarAlCarro(string id, string nombre, int canti)
        {
            var TraeProductos = (from pr in db.Productos
                                 where pr.ItemBarCode == id
                                 select pr);

            return Json(TraeProductos.FirstOrDefault().ItemName,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ElCarro(string id, string nombre, int canti, string kit)
        {
            //if el producto tiene kit.. agregar los 2
            var TraeProductos = (from pr in db.Productos
                                where pr.ItemBarCode == id
                                select pr);

            var traeprod = TraeProductos.FirstOrDefault();

            var getRecRefId = traeprod.RefRecId;
            long getRRI = Convert.ToInt64(getRecRefId);

            string NombreProducto = traeprod.ItemName + " " + traeprod.brandInternet;
            
            ViewBag.ftec = traeprod.RefRecId;

            if (Session["cart"] == null)
            {
                AddCabecera();
            }

            string npedido = Session["cart"].ToString();
            int np = Convert.ToInt32(npedido);

            var Carrito = from cr in db.DetalleCarro
                          where cr.ItemBarCode == id && cr.NPedido == np
                          select cr;
            string Agregado = string.Empty;
            if (Session["cart"] != null )
            {
                var mycart = Session["cart"].ToString();
                int ncart = Convert.ToInt32(mycart);
                if (Carrito.Any() == false)
                    {
                    //aca revisar si tiene factor , cantidad por el valor de la caja
                        if (traeprod.Factor != 0)
                        {

                            var Mt2Acomprar = traeprod.Factor * canti;//este se lo paso al AX
                            var getValorCaja = traeprod.Factor * traeprod.Price;//Se lo muestro al cliente
                            var subtotalxcaja = getValorCaja * canti;
                            int valorCaja = Convert.ToInt32(getValorCaja);
                            int subtotalCaja = Convert.ToInt32(subtotalxcaja);

                            DetalleCarro cart = new DetalleCarro
                            {
                                NPedido = Convert.ToInt32(mycart),
                                ItemBarCode = traeprod.ItemBarCode,
                                ItemName = NombreProducto,
                                Precio = valorCaja,
                                Cantidad = canti,
                                Subtotal = subtotalCaja,
                                Photo = traeprod.Photo,
                                RecRefId = traeprod.RefRecId
                            };
                            db.DetalleCarro.Add(cart);
                            try
                            {
                                db.SaveChanges();
                                Agregado = "Agregado al carro";
                                ViewBag.listo = "Agregado";
                            }
                            catch (Exception e)
                            {
                                ViewBag.error = (e.Message);
                            }
                        }
                        else
                        {
                            //revisar si tiene kit!
                            if (kit != null)
                            {
                                if (ProductosKit(getRRI) == true)
                                {
                                    //insert 2 productos
                                    DetalleCarro cart = new DetalleCarro
                                    {
                                        NPedido = Convert.ToInt32(mycart),
                                        ItemBarCode = traeprod.ItemBarCode,
                                        ItemName = NombreProducto,
                                        Precio = traeprod.Price,
                                        Cantidad = canti,
                                        Subtotal = canti * Convert.ToInt32(traeprod.Price),
                                        Photo = traeprod.Photo,
                                        RecRefId = traeprod.RefRecId,
                                        
                                    };
                                    db.DetalleCarro.Add(cart);
                                    try
                                    {
                                        db.SaveChanges();
                                        if (agregarProductoKit(ncart, canti) == true)
                                        {
                                            Agregado = "Agregado";
                                            ViewBag.listo = "Agregado";
                                        }
                                        else
                                        {
                                            ViewBag.error = "No se agregó producto kit";
                                        }
                                        
                                    }
                                    catch (Exception e)
                                    {
                                        ViewBag.error = (e.Message);
                                    }
                                }
                                else
                                {
                                    //sin kit
                                    DetalleCarro cart = new DetalleCarro
                                    {
                                        NPedido = Convert.ToInt32(mycart),
                                        ItemBarCode = traeprod.ItemBarCode,
                                        ItemName = NombreProducto,
                                        Precio = traeprod.Price,
                                        Cantidad = canti,
                                        Subtotal = canti * Convert.ToInt32(traeprod.Price),
                                        Photo = traeprod.Photo,
                                        RecRefId = traeprod.RefRecId,

                                    };
                                    db.DetalleCarro.Add(cart);
                                    try
                                    {

                                        db.SaveChanges();
                                        Agregado = "Agregado";
                                        ViewBag.listo = "Agregado";
                                    }
                                    catch (Exception op)
                                    {
                                        ViewBag.error = op.Message;
                                    }
                                }
                                

                            }
                            else
                            {
                                //si no tiene kit agregar normal
                                DetalleCarro cart = new DetalleCarro
                                {
                                    NPedido = Convert.ToInt32(mycart),
                                    ItemBarCode = traeprod.ItemBarCode,
                                    ItemName = NombreProducto,
                                    Precio = traeprod.Price,
                                    Cantidad = canti,
                                    Subtotal = canti * Convert.ToInt32(traeprod.Price),
                                    Photo = traeprod.Photo,
                                    RecRefId = traeprod.RefRecId
                                };
                                db.DetalleCarro.Add(cart);
                                try
                                {
                                    db.SaveChanges();
                                    Agregado = "Agregado";
                                    ViewBag.listo = "Agregado";
                                }
                                catch (Exception e)
                                {
                                    ViewBag.error = (e.Message);
                                }
                            }
                            
                        }
                        

                        
                    }
                    else
                    {
                    //si ya existe el producto y tiene kit... 
                        var yaexiste = Carrito.FirstOrDefault();
                        yaexiste.Cantidad++;
                        try
                        {
                            ViewBag.listo = "Agregado";
                            db.SaveChanges();
                            
                        }
                        catch (Exception e)
                        {
                            Response.Write(e.Message);
                        }

                        var elprecio = yaexiste.Precio;
                        var subtotal = Convert.ToInt32(elprecio) * yaexiste.Cantidad;
                        yaexiste.Subtotal = subtotal;
                        try
                        {
                            db.SaveChanges();
                            ViewBag.listo = "Actualizado";
                        }
                        catch (Exception e)
                        {
                            ViewBag.error = (e.Message);
                        }

                    }
            }

            return Json(Agregado, JsonRequestBehavior.AllowGet);
        }

        public bool agregarProductoKit(int mycart, int canti)
        {
            var queryProdKit = (from kt in db.ProductosKit
                                select kt);
            string NameprodKit = string.Empty;
            
            DetalleCarro cartKit = new DetalleCarro();
            foreach (var kk in queryProdKit)
            {
                NameprodKit = kk.ItemName + " " +kk.brandInternet;
                cartKit.NPedido = Convert.ToInt32(mycart);
                cartKit.ItemBarCode = kk.ItemName;
                cartKit.ItemName = NameprodKit;
                cartKit.Precio = kk.Price;
                cartKit.Cantidad = canti;
                cartKit.Subtotal = canti * kk.Price;
                cartKit.Photo = kk.Photo;
                cartKit.RecRefId = kk.RefRecId;
                
            }
            db.DetalleCarro.Add(cartKit);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                ViewBag.error = (e.Message);
                return false;
            }
            
        }
        public bool ProductosKit(long id)
        {
            if (DeleteProductoKit(id) == true)
            {
                var querytempProd = (from pr in db.tempProdKit
                                     select pr).ToList();

                foreach (var i in querytempProd)
                {
                    db.tempProdKit.Remove(i);
                }
                db.SaveChanges();

                if (CargaProductoKit() == true)
                {
                    var queryProductoKit = (from prodKit in db.tempProdKit
                                            join prod in db.Productos on prodKit.RefRecId equals prod.RefRecId
                                            where prodKit.RefRecId == id
                                            select prodKit).ToList();//productos producto que debe encontrar
                    int count = 0;

                    //si no tiene productos, los agrego
                    foreach (var item in queryProductoKit)
                    {

                        var query = (from pr in db.Productos
                                     where pr.ItemId == item.ItemId && pr.ConfigId == item.ConfigId && pr.InventSizeId == item.InventSizeId
                                     && pr.InventColorId == item.InventColorId
                                     select pr);

                        var qr = query.FirstOrDefault();
                        if (query.Any() == true)
                        {
                            //insert en temporal
                            ProductosKit temp = new ProductosKit
                            {
                                IdProd = count++,
                                ItemId = qr.ItemId,
                                ItemBarCode = qr.ItemBarCode,
                                ItemName = qr.ItemName,
                                Clasif1 = qr.Clasif1,
                                Clasif2 = qr.Clasif2,
                                Clasif3 = qr.Clasif3,
                                ConfigId = qr.ConfigId,
                                InventColorId = qr.InventColorId,
                                InventSizeId = qr.InventSizeId,
                                Price = qr.Price,
                                Photo = qr.Photo,
                                HighLights = qr.HighLights,
                                ICat3 = qr.ICat3,
                                Stock = qr.Stock,
                                brandInternet = qr.brandInternet,
                                UnitWeb = qr.UnitWeb,
                                Factor = qr.Factor,
                                Orden = qr.Orden,
                                RefRecId = qr.RefRecId
                            };

                            db.ProductosKit.Add(temp);
                            try
                            {
                                db.SaveChanges();
                                return true;
                            }
                            catch (Exception e)
                            {
                                ViewBag.error = (e.Message);
                            }

                        }

                    }

                }
                else
                {
                    ViewBag.error = "No se cargaron los kits";
                }
            }



            return false;
        }
        public bool DeleteProductoKit(long id)
        {
            var queryProd = (from pr in db.ProductosKit
                             select pr).ToList();

            foreach (var i in queryProd)
            {
                db.ProductosKit.Remove(i);
            }
            db.SaveChanges();
            return true;
        }
        public bool deleteTempKit()
        {
            var queryProductoKit = (from pdrel in db.tempProdKit
                                    select pdrel).ToList();
            foreach (var it in queryProductoKit)
            {
                db.tempProdKit.Remove(it);
            }
            db.SaveChanges();
            return true;
        }
        public bool CargaProductoKit()
        {
            AxaptaRecord axRecord;

            var queryProductoKit = (from pdrel in db.tempProdKit
                                    select pdrel).ToList();

            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            if (deleteTempKit() == true)
            {
                using (axRecord = DynAx.CreateAxaptaRecord("InventConbinationItemKit"))
                {
                    axRecord.ExecuteStmt("select * from %1");
                    if (queryProductoKit.Count == 0)
                    {
                        int count = 0;
                        while (axRecord.Found)
                        {

                            tempProdKit tempProdKit = new tempProdKit
                            {
                                IdProd = count++,
                                ItemId = Convert.ToString(axRecord.get_Field("ItemId")),
                                ConfigId = Convert.ToString(axRecord.get_Field("ConfigId")),
                                InventSizeId = Convert.ToString(axRecord.get_Field("InventSizeId")),
                                InventColorId = Convert.ToString(axRecord.get_Field("InventColorId")),
                                RefRecId = Convert.ToInt64(axRecord.get_Field("RefRecId")),

                            };
                            db.tempProdKit.Add(tempProdKit);

                            axRecord.Next();
                        }
                        db.SaveChanges();
                        DynAx.Logoff();
                        return true;
                    }

                }
            }

            return false;
        }

        [HttpPost]
        public JsonResult SacarProd(string id)
        {
            if (Session["cart"] != null)
            {
                string npedido = Session["cart"].ToString();

                var cartItem = (from cr in db.DetalleCarro
                                where cr.ItemBarCode == id && cr.NPedido.ToString() == npedido
                                select cr).FirstOrDefault();
                //int itemCount = 0;
                if (cartItem != null)
                {
                    if (cartItem.Cantidad > 1)
                    {
                        db.DetalleCarro.Remove(cartItem);
                    }
                    else
                    {
                        db.DetalleCarro.Remove(cartItem);

                    }
                    db.SaveChanges();
                }
            }
            

            return Json(db.DetalleCarro.ToList(),JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeleteItem(string id)
        {

           
            //int itemCount = 0;
            if (Session["cart"] != null)
            {
                string npedido = Session["cart"].ToString();

                var cartItem = (from cr in db.DetalleCarro
                                where cr.ItemBarCode == id && cr.NPedido.ToString() == npedido
                                select cr).FirstOrDefault();
                //int itemCount = 0;
                if (cartItem != null)
                {
                    if (cartItem.Cantidad > 1)
                    {
                        db.DetalleCarro.Remove(cartItem);
                    }
                    else
                    {
                        db.DetalleCarro.Remove(cartItem);

                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception kj)
                    {
                        ViewBag.error = kj.Message;
                    }
                    
                }
            }

            return RedirectToAction("CarroDeCompras", "Compra", db.DetalleCarro);
        }

        public ActionResult VaciarCarro(int id)
        {

            var cartItems = from p in db.DetalleCarro
                            where p.NPedido == id
                            select p;

            foreach (var item in cartItems)
            {
                db.DetalleCarro.Remove(item);
            }
            db.SaveChanges();
            return View("ElCarro", db.DetalleCarro);
        }

        [HttpPost]
        public ActionResult ActualizaCantidad(string barcode, int quantity)
        {
            string npedido = Session["cart"].ToString();
            int np = Convert.ToInt32(npedido);

            var losproductos = (from p in db.DetalleCarro
                                         where p.ItemBarCode == barcode && p.NPedido == np
                                         select p).FirstOrDefault();

            var getFactorProducto = (from pr in db.Productos
                                    join dc in db.DetalleCarro on pr.ItemBarCode equals dc.ItemBarCode
                                    where pr.ItemBarCode == barcode && dc.NPedido == np
                                    select pr).FirstOrDefault();
            
            if (quantity <= 0)
            {
                var cartItem = (from p in db.DetalleCarro
                                where p.ItemBarCode == barcode 
                                select p).FirstOrDefault();
                //int itemCount = 0;
                if (cartItem != null)
                {
                    if (cartItem.Cantidad > 1)
                    {
                        db.DetalleCarro.Remove(cartItem);
                    }
                    else
                    {
                        db.DetalleCarro.Remove(cartItem);

                    }
                    db.SaveChanges();
                }
            }
            else
            {
                losproductos.Cantidad = quantity;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ep)
                {
                    ViewBag.error = (ep.Message);
                }

                if (getFactorProducto.Factor != 0)
                {
                    //el subtotal se multiplica por el valor de la caja 
                    var valorCaja = getFactorProducto.Factor * getFactorProducto.Price;
                    var cantiXcaja = valorCaja * quantity;
                    Convert.ToInt32(cantiXcaja);

                    var elsubtotal = Convert.ToInt32(cantiXcaja);
                    losproductos.Subtotal = Convert.ToInt32(elsubtotal);
                    try
                    {
                        db.SaveChanges();
                       
                    }
                    catch (Exception ep)
                    {
                        ViewBag.error = (ep.Message);
                    }
                }
                else
                {
                    var elsubtotal = losproductos.Precio * losproductos.Cantidad;
                    losproductos.Subtotal = Convert.ToInt32(elsubtotal);
                    try
                    {
                        db.SaveChanges();

                    }
                    catch (Exception ep)
                    {
                        ViewBag.error = (ep.Message);
                    }
                   
                }

            }

            return RedirectToAction("CarroDeCompras", "Compra", db.DetalleCarro);
        }

        public ActionResult ResumenCompra(int id, string rutusuario)
        {
            //traer carro pagado con npedid y cliente
            //enviar correo al cliente con detalle de lo que pago.
           
            ViewBag.id = id;
            Session["cart"] = null;
            var traecarropagado = (from cr in db.DetalleCarro
                                   where cr.NPedido == id && cr.CabeceraCarro.RutCliente == rutusuario
                                   select cr);

            return View(traecarropagado.ToList());
            
        }


    }
}
