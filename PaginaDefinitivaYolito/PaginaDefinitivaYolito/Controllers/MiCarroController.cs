using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;

namespace PaginaDefinitivaYolito.Controllers
{
    public class MiCarroController : Controller
    {
        //
        // GET: /MiCarro/
        private PaginaWebEntities1 db = new PaginaWebEntities1();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ComboComuna(string id)
        {
            var ComunaRegion = db.AddressCounty.Where(c => c.StateId == id);
            ViewBag.Comunas = ComunaRegion.ToList();
            return Json(new SelectList(ComunaRegion, "STATEID", "NAME"));
        }

        //[HttpGet]
        //public ActionResult MiCarroDeCompras()
        //{
            
        //    var losproductos = (from p in db.DetalleCarro
        //                        join c in db.CabeceraCarro on p.NPedido equals c.NPedido
        //                        where c.RutCliente == User.Identity.Name
        //                        && c.EstadoOc == false
        //                        select p).Any();

        //    ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c=> c.Name).ToList();
        //    //ViewBag.Regiones = db.ADDRESSSTATE.OrderBy(r => r.NAME).ToList();
        //    //ViewBag.Pais = db.ADDRESSCOUNTRYREGION.ToList();
        //    if (losproductos == false)
        //    {
        //        return View("MiCarroVacio", db.DetalleCarro.Where(c => c.CabeceraCarro.RutCliente == User.Identity.Name && c.CabeceraCarro.EstadoOc == false).FirstOrDefault());
        //    }
        //    else
        //    {
        //        return View("MiCarro", db.DetalleCarro);
        //    }
            
        //}

        //public ActionResult rendercarro()
        //{
        //    var losproductos = (from p in db.DetalleCarro
        //                        join c in db.CabeceraCarro on p.NPedido equals c.NPedido
        //                        where c.RutCliente == User.Identity.Name
        //                        && c.EstadoOc == false
        //                        select p).Any();

        //    ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
        //    //ViewBag.Regiones = db.ADDRESSSTATE.OrderBy(r => r.NAME).ToList();
        //    //ViewBag.Pais = db.ADDRESSCOUNTRYREGION.ToList();
        //    if (losproductos == false)
        //    {
        //        return PartialView(db.DetalleCarro.Where(c => c.CabeceraCarro.RutCliente == User.Identity.Name && c.CabeceraCarro.EstadoOc == false).FirstOrDefault());
        //    }
        //    else
        //    {
        //        return PartialView(db.DetalleCarro);
        //    }
        //}


        public ActionResult MediosDePago()
        {
            return View();
        }


        //public ActionResult Agregaralcarro(string id, string nombre)
        //{
        //    var query_usuario = (from u in db.Clientes
        //                         where u.RutCliente + "-" + u.DigCliente == User.Identity.Name
        //                         select u).ToList();

        //    var queryProductos = (from p in db.Productos
        //                         where p.ItemBarCode == id
        //                         select p).ToList();

        //    var losproductos = (from p in db.DetalleCarro
        //                        join c in db.CabeceraCarro on p.NPedido equals c.NPedido
        //                        where p.ItemBarCode == id && c.RutCliente == User.Identity.Name
        //                        && c.EstadoOc == false
        //                        select p).FirstOrDefault();

        //    if (User.Identity.IsAuthenticated)//si esta autenticado el usuario
        //    {   
        //        //consulta si tiene cabecera.
        //        var sitienecabecera = (from cabecera in db.CabeceraCarro
        //                              where cabecera.RutCliente == User.Identity.Name
        //                              && cabecera.EstadoOc == false
        //                              select cabecera).FirstOrDefault();
                
        //        if (sitienecabecera == null) //si no tiene cabecera
        //        {
        //            var insertaCabecera = db.CabeceraCarro.Create();
        //            foreach (var q in query_usuario)
        //            {

        //                insertaCabecera.RutCliente = User.Identity.Name;
        //                insertaCabecera.NombreCliente = q.Nombre;
        //                insertaCabecera.EstadoOc = false;
        //            }
        //            db.CabeceraCarro.Add(insertaCabecera);
        //            db.SaveChanges();

                    

        //            if (losproductos == null) //si el carro esta vacio.
        //            {   
        //                double pr = 0;
        //                var pedido_cabecera = db.CabeceraCarro.Where(c => c.RutCliente == User.Identity.Name
        //                && c.EstadoOc == false).FirstOrDefault();
        //                var insertaProducto = db.DetalleCarro.Create();
        //                foreach (var item in queryProductos)
        //                {
        //                    insertaProducto.NPedido = pedido_cabecera.NPedido;
        //                    insertaProducto.ItemName = item.ItemName;
        //                    string precio = string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:#,##}", Math.Floor(Convert.ToDecimal(item.Price)));
        //                    pr = Double.Parse(precio);
        //                    string ppr = Convert.ToString(pr);
        //                    insertaProducto.ItemBarCode = item.ItemBarCode;
        //                    insertaProducto.ItemName = item.ItemName + " " + item.ConfigId + " " + item.InventColorId + " " + item.InventSizeId;
        //                    insertaProducto.Precio = ppr;
        //                    insertaProducto.Cantidad = 1;
        //                    var elsubtotal = Double.Parse(precio) * insertaProducto.Cantidad;
        //                    insertaProducto.Subtotal = Convert.ToInt32(elsubtotal);
        //                    insertaProducto.Photo = item.Photo;
        //                }
        //                db.DetalleCarro.Add(insertaProducto);
        //                try
        //                {
        //                    db.SaveChanges();
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.Write(e.Message);
        //                }
                        
        //            }//el carro no esta vacio
        //            else
        //            {
        //                if (losproductos.CabeceraCarro.RutCliente == User.Identity.Name)
        //                {
        //                    losproductos.Cantidad++;
        //                    db.SaveChanges();
        //                    var elprecio = Double.Parse(losproductos.Precio);
        //                    var elsubtotal = elprecio * losproductos.Cantidad;
        //                    losproductos.Subtotal = Convert.ToInt32(elsubtotal);
        //                    db.SaveChanges();
        //                }
                        
        //            }

        //        }//si tiene cabecera
        //        else
        //        {   
        //            var pedido_cabecera = (from cb in db.CabeceraCarro
        //                                       where cb.RutCliente == User.Identity.Name
        //                                       && cb.EstadoOc == false
        //                                       select cb).SingleOrDefault();
        //            //var pedido_cabecera = db.CabeceraCarro.Where(c => c.RutCliente == User.Identity.Name
        //            //    && c.EstadoOc == false).FirstOrDefault();

                    
        //            if (losproductos == null) //si el carro esta vacio.
        //            {
                        
        //                var productos1 = (from p in db.Productos
        //                                 where p.ItemBarCode == id 
        //                                 select p).ToList();

        //                var pp = productos1.First();
        //                if (queryProductos.Any() == false)
        //                {
        //                    Response.Write("NOENCONTRADO");
        //                }
        //                else
        //                {

        //                    var insertaProducto = db.DetalleCarro.Create();
        //                    insertaProducto.NPedido = pedido_cabecera.NPedido;
        //                    insertaProducto.ItemBarCode = pp.ItemBarCode;
        //                    insertaProducto.ItemName = pp.ItemName + " " + pp.ConfigId + " " + pp.InventSizeId + " " + pp.InventColorId;
        //                    insertaProducto.Cantidad = 1;
        //                    string precio = string.Format(new System.Globalization.CultureInfo("es-CL"), "{0:#,##}", Math.Floor(Convert.ToDecimal(pp.Price)));
        //                    double pr1 = 0;
        //                    pr1 = Double.Parse(precio);
        //                    string ppr = Convert.ToString(pr1);
        //                    insertaProducto.Precio = ppr;
        //                    insertaProducto.Photo = pp.Photo;
        //                    var elsubtotal = pr1 * insertaProducto.Cantidad;
        //                    insertaProducto.Subtotal = Convert.ToInt32(elsubtotal);
        //                    db.DetalleCarro.Add(insertaProducto);
        //                    try
        //                    {
        //                        db.SaveChanges();
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Response.Write(e.Message + "" + e.InnerException);
        //                    }
                            
        //                }
        //            }//el carro no esta vacio
        //            else
        //            {
        //                if (losproductos.CabeceraCarro.RutCliente == User.Identity.Name)
        //                {
        //                    losproductos.Cantidad++;
        //                    db.SaveChanges();
        //                    var elprecio = Double.Parse(losproductos.Precio);
        //                    var elsubtotal = elprecio * losproductos.Cantidad;
        //                    losproductos.Subtotal = Convert.ToInt32(elsubtotal);
        //                    db.SaveChanges();
        //                }

        //            }
                    
        //        }

        //    }
        //    return View("MiCarro", db.DetalleCarro);
        //}

        public ActionResult DeleteItem(string id)
        {
            
            var cartItem = (from p in db.DetalleCarro
                            join c in db.CabeceraCarro on p.NPedido equals c.NPedido
                            where p.ItemBarCode == id && c.EstadoOc == false
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
            
            return View("ElCarro", db.DetalleCarro);
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
            DetalleCarro losproductos = (from p in db.DetalleCarro
                                where p.ItemBarCode == barcode 
                                select p).Single();

            double elprecio = Double.Parse(losproductos.Precio);
            if (quantity <= 0)
            {
                var cartItem = (from p in db.DetalleCarro
                                join c in db.CabeceraCarro on p.NPedido equals c.NPedido
                                where p.ItemBarCode == barcode && c.RutCliente == User.Identity.Name && c.EstadoOc == false
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
                if (losproductos != null)
                {
                    losproductos.Cantidad = quantity;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ep)
                    {
                        Console.Write(ep.Message);
                    }
                    var elsubtotal = elprecio * losproductos.Cantidad;
                    losproductos.Subtotal = Convert.ToInt32(elsubtotal);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ep)
                    {
                        Console.Write(ep.Message);
                    }

                }

            }
            
            return View("MiCarro",db.DetalleCarro);
        }

        public ActionResult AgregaUno(string id)
        {
            
            var losproductos = (from p in db.DetalleCarro
                                join c in db.CabeceraCarro on p.NPedido equals c.NPedido
                                where p.ItemBarCode == id && c.RutCliente == User.Identity.Name
                                && c.EstadoOc == false
                                select p).SingleOrDefault();
            if (losproductos != null)
            {
                losproductos.Cantidad++;
                db.SaveChanges();
                var elprecio = Double.Parse(losproductos.Precio);
                var elsubtotal = elprecio * losproductos.Cantidad;
                losproductos.Subtotal = Convert.ToInt32(elsubtotal);
                db.SaveChanges();
            }

            return View(db.DetalleCarro);
        }

        public ActionResult QuitaUno(string id)
        {
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
            var cartItem = (from p in db.DetalleCarro
                            join c in db.CabeceraCarro on p.NPedido equals c.NPedido
                            where p.ItemBarCode == id && c.RutCliente == User.Identity.Name && c.EstadoOc == false
                            select p).SingleOrDefault();
            int itemCount = 0;
            if (cartItem != null)
            {
                if (cartItem.Cantidad > 1)
                {
                    cartItem.Cantidad--;
                    itemCount = cartItem.Cantidad;
                }
                else
                {
                    db.DetalleCarro.Remove(cartItem);

                }
                db.SaveChanges();
            }
            return View("MiCarro", db.DetalleCarro);
        }

        [HttpGet]
        public ActionResult Despacho(int id)
        {
            ViewBag.ncarro = id;
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
            
            return View();
        }

    }
}
