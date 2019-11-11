 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.Net.Mail;


namespace PaginaDefinitivaYolito.Controllers
{
    public class ConfirmaCompraController : Controller
    {
        //
        // GET: /ConfirmaCompra/
        private PaginaWebEntities1 db = new PaginaWebEntities1();
        private Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();

        public ActionResult Index()
        {
            return View();
        }

        

        [HttpGet]
        public ActionResult Despacho(int id)
        {
            
            ViewBag.ncarro = id;
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();

            //if (Session["rutuser"] != null)
            //{
            //    //si esta registrado arrojar a otra accion de controlador
            //    ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
                
            //    return View("Despacho");
            //}
            //else
            //{
            //    //si no esta registrado enviar a otro controlador
            //    return PartialView("DespachoCliente");
            //}
            return PartialView("DespachoCliente");
            
        }

        [HttpPost]
        public ActionResult MetodoDePago(string radiobt, string rut, string NombreDespacho, int ncarro, string Retira, string Local, string Responsable, string Comunas, string Calle, string Obs, string Despacho1, string Email, string EmailRetira, string tipoDocumento)
        {

            var totalCompra = (from d in db.DetalleCarro
                               where d.NPedido == ncarro && d.CabeceraCarro.EstadoOc == false
                               select d.Subtotal).Sum();


            var CarroCliente = from c in db.DetalleCarro
                               where c.NPedido == ncarro && c.CabeceraCarro.EstadoOc == false
                               select c;

            ViewBag.RutUsuariocomprador = rut;

            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

            object o;
            if (radiobt == "despacho")
            {
                TempData["rutDespacho"] = rut;
                
                //el proceso de despacho
                object o1;
                string itemcode = string.Empty;
                string canti = string.Empty;

                try
                {
                   
                    foreach (var p in CarroCliente)
                    {
                        itemcode += p.ItemBarCode + "|";
                        canti += p.Cantidad + "|";
                    }
                    o1 = DynAx.CallStaticClassMethod("WebPageConection", "CalcFreight", itemcode, canti);

                    string valorflete = o1.ToString();

                    var Despacho = from c in db.CabeceraCarro
                                   where c.NPedido == ncarro && c.EstadoOc == false
                                   select c;

                    int montodesp = Convert.ToInt32(valorflete) + totalCompra;
                    foreach (var i in Despacho)
                    {
                        i.RutCliente = rut;
                        i.NombreCliente = NombreDespacho;
                        i.ComunaDespacho = Comunas;
                        i.CalleDespacho = Calle;
                        i.ObservacionDespacho = Obs;
                        i.CostoDespacho = valorflete;
                        i.ModoEntrega = Despacho1;
                        i.Local = "03";
                        i.EmailCliente = Email;
                        i.MontoPagado = montodesp.ToString();
                        i.tipoDocumento = tipoDocumento;

                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception p)
                    {
                        ViewBag.error = "[01] " + p.Message;
                    }


                    foreach (var c in CarroCliente)
                    {
                        o = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock", c.ItemBarCode,
                            "03", c.Cantidad);
                        //Response.Write(c.ItemBarCode + "<br/>");
                        if (o.ToString() == "1")
                        {
                            //Response.Write(totalCompra);
                            int dp = Convert.ToInt32(valorflete);
                            ViewBag.despacho = dp;
                            return View("MetodoDePago", db.DetalleCarro);
                        }
                        else
                        {
                            TempData["error"] = "Producto no disponible para despacho";
                            return RedirectToAction("Despacho", new { id = c.NPedido });
                        }
                    }

                    DynAx.Logoff();

                }
                catch (Exception ex)
                {
                    ViewBag.error = "[3]" + ex.Message;
                }


            }
            else
            {
                //proceso retira en tienda
                
                var Despacho = from c in db.CabeceraCarro
                               where c.NPedido == ncarro && c.EstadoOc == false
                               select c;

                foreach (var i in Despacho)
                {
                    i.RutCliente = rut;
                    i.ResponsableDespacho = Responsable;
                    i.RutResponsable = rut;
                    i.ModoEntrega = Retira;
                    i.Local = Local;
                    i.EmailCliente = EmailRetira;
                    i.MontoPagado = totalCompra.ToString();
                    i.tipoDocumento = tipoDocumento;

                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ep)
                {
                    ViewBag.error = ep.Message;
                }
                
                foreach (var c in CarroCliente)
                {
                    

                    o = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock", c.ItemBarCode,
                        Local, c.Cantidad);
                    //Response.Write(c.ItemBarCode + "<br/>");
                    if (o.ToString() == "1")
                    {
                        ViewBag.Comunas = db.AddressCounty.Where(c1 => c1.DlvRoute != "").OrderBy(c1 => c1.Name).ToList();
                        //Response.Write(totalCompra);
                        ViewBag.Tienda = Local;
                        return View("MetodoDePago",db.DetalleCarro);
                    }
                    else
                    {
                        TempData["error"] = "Producto no disponible en la tienda";
                        return RedirectToAction("Despacho", new { id = c.NPedido });
                    }
                }
                DynAx.Logoff();

            }

            return View(db.DetalleCarro);
        }

        [HttpGet]
        public ActionResult TipoPago()
        {
            
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
            return View("TipoPago", db.DetalleCarro);
        }


        public ActionResult TipopagoRet(string Responsable, string rut, int ncarro, string radiobt, string Local, string tipoDocumento)
        {
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
            
            ViewBag.despacho = "Retira en tienda";
            if (Session["cart"] != null)
            {   
                object o;
                var totalCompra = (from d in db.DetalleCarro
                                   where d.NPedido == ncarro && d.CabeceraCarro.EstadoOc == false
                                   select d.Subtotal).Sum();

                
                var Despacho = (from c in db.CabeceraCarro
                               where c.NPedido == ncarro && c.EstadoOc == false
                               select c).FirstOrDefault();

                var CarroCliente = from c in db.DetalleCarro
                                   where c.NPedido == ncarro && c.CabeceraCarro.EstadoOc == false
                                   select c;

                Despacho.RutCliente = Session["rutuser"].ToString();
                Despacho.NombreCliente = Session["nombreuser"].ToString();
                Despacho.ResponsableDespacho = Responsable;
                Despacho.RutResponsable = rut;
                Despacho.ModoEntrega = radiobt;
                Despacho.Local = Local;
                Despacho.MontoPagado = totalCompra.ToString();
                Despacho.tipoDocumento = tipoDocumento;
                try
                {
                    db.SaveChanges();
                }
                catch(Exception ep)
                {
                    ViewBag.error = ep.Message;
                }
                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                foreach (var c in CarroCliente)
                {
                    

                    o = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock", c.ItemBarCode,
                        Local, c.Cantidad);
                    //Response.Write(c.ItemBarCode + "<br/>");
                    if (o.ToString() == "1")
                    {
                        ViewBag.Comunas = db.AddressCounty.Where(c1 => c1.DlvRoute != "").OrderBy(c1 => c1.Name).ToList();
                        //Response.Write(totalCompra);
                        return View(db.DetalleCarro);
                    }
                    else
                    {
                        TempData["error"] = "Producto no disponible en la tienda";
                        return RedirectToAction("Despacho", new { id = c.NPedido });
                    }

                }

                DynAx.Logoff();
                
            }

            return View(db.DetalleCarro);
        }

        [HttpPost]
        public ActionResult TipoPago(string Comunas, string Calle, string Obs, int ncarro, string Despacho1, string tipoDocumento)
        {
            
            ViewBag.Comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();
            if (ModelState.IsValid)
            {
                if (Session["cart"] != null)
                {
                    
                    object o;
                    object o1;
                    string itemcode = string.Empty;
                    string canti = string.Empty;


                    var CarroCliente = from c in db.DetalleCarro
                                       where c.NPedido == ncarro && c.CabeceraCarro.EstadoOc == false
                                       select c;

                    var totalCompra = (from d in db.DetalleCarro
                                       where d.NPedido == ncarro && d.CabeceraCarro.EstadoOc == false
                                       select d.Subtotal).Sum();
                    
                    try
                    {
                        Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                        System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                        DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                        try
                        {
                            //ingresar el pedido.
                            foreach (var p in CarroCliente)
                            {
                                itemcode += p.ItemBarCode + "|";
                                canti += p.Cantidad + "|";
                            }
                            o1 = DynAx.CallStaticClassMethod("WebPageConection", "CalcFreight", itemcode, canti);

                            string valorflete = o1.ToString();

                            var Despacho = from c in db.CabeceraCarro
                                           where c.NPedido == ncarro && c.EstadoOc == false
                                           select c;

                            int montodesp = Convert.ToInt32(valorflete) + totalCompra ;
                            foreach (var i in Despacho)
                            {
                                i.RutCliente = Session["rutuser"].ToString();
                                i.NombreCliente = Session["nombreuser"].ToString();
                                i.ComunaDespacho = Comunas;
                                i.CalleDespacho = Calle;
                                i.ObservacionDespacho = Obs;
                                i.CostoDespacho = valorflete;
                                i.ModoEntrega = Despacho1;
                                i.Local = "03";
                                i.MontoPagado = montodesp.ToString();
                                i.tipoDocumento = tipoDocumento;
                                
                            }

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception p)
                            {
                                ViewBag.error = "[01] " + p.Message;
                            }
                            

                            foreach (var c in CarroCliente)
                            {
                                o = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock", c.ItemBarCode,
                                    "03", c.Cantidad);
                                //Response.Write(c.ItemBarCode + "<br/>");
                                if (o.ToString() == "1")
                                {

                                    //Response.Write(totalCompra);
                                    int dp = Convert.ToInt32(valorflete);
                                    ViewBag.despacho = dp;
                                    return View(db.DetalleCarro);
                                }
                                else
                                {
                                    TempData["error"] = "Producto no disponible en la tienda";
                                    return RedirectToAction("Despacho", new { id = c.NPedido });
                                }
                            }
                            DynAx.Logoff();
                        }
                        catch (Exception ex)
                        {
                            ViewBag.error= "[2] "+ex.Message;
                        }

                        DynAx.Logoff();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.error =  "[3]"+ex.Message;
                    }
                }
                
            }
            else
            {
                ViewBag.error = "Error en formulario";
                
            }
            
            return View(db.DetalleCarro);
        }

        //public ActionResult ConfirmarTransferenciaSinRegistro(int id, string rutcliente)
        //{
        //    DateTime hoydia = DateTime.Today;
        //    string fecha = hoydia.ToString("MMdd");
        //    string fecha_exito = hoydia.ToString("dd/MM/yyyy");

        //    if (Session["cart"] != null)
        //    {
        //        string npedido = Session["cart"].ToString();
        //        //string rutcliente = Session["rutuser"].ToString();
        //        int np = Convert.ToInt32(npedido);

        //        var carrito = (from c in db.CabeceraCarro
        //                       where c.NPedido == np && c.EstadoOc == false
        //                       select c).FirstOrDefault();

        //        carrito.EstadoOc = true;
        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception ep)
        //        {
        //            ViewBag.error = "[4]" + ep.Message;
        //        }

        //        if (IngresaTransferenciaAx(np, rutcliente, fecha_exito) == true)
        //        {
        //            ViewBag.ExitoT = "Enviada";
        //            ViewBag.npedido = np;
        //            Session["cart"] = null;
        //            return View("ConfirmarTransferenciaSinRegistro");
        //        }
        //        else
        //        {
        //            ViewBag.NoIngresa = "No se ingresó la transferencia";
        //            return View("MetodoDePago");
        //        }

        //    }
        //    else
        //    {
        //        ViewBag.errorC = "Carro expiró";
        //    }
        //    return View();
        //}

        //public ActionResult ConfirmarTransferencia(int id , string rutcliente)
        //{
        //    DateTime hoydia = DateTime.Today;
        //    string fecha = hoydia.ToString("MMdd");
        //    string fecha_exito = hoydia.ToString("dd/MM/yyyy");

        //    var carrito = (from c in db.CabeceraCarro
        //                   where c.NPedido == id && c.EstadoOc == false
        //                   select c).FirstOrDefault();

        //    var OrdenesPagadas1 = (from c in db.CabeceraCarro
        //                           from cli in db.Clientes
        //                           where c.NPedido == id && c.EstadoOc == true
        //                           select cli).FirstOrDefault();

        //    if (Session["cart"] != null)
        //    {
        //        if (Session["rutuser"] == null)
        //        {
        //            carrito.EstadoOc = true;
        //            try
        //            {
        //                db.SaveChanges();
        //            }
        //            catch (Exception ep)
        //            {
        //                ViewBag.error = "[4]" + ep.Message;
        //            }

        //            if (IngresaTransferenciaAx(id, rutcliente, fecha_exito) == true)
        //            {
        //                ViewBag.ExitoT = "Enviada";
        //                ViewBag.npedido = id;
        //                Session["cart"] = null;
        //                //enviar correo

        //                var ProductosComprados = from prd in db.DetalleCarro
        //                                         where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
        //                                         select prd;



        //                SmtpClient smtp = new SmtpClient();
        //                smtp.Host = "smtp.gmail.com";
        //                smtp.Port = 587;
        //                smtp.EnableSsl = true;
        //                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //                smtp.UseDefaultCredentials = false;
        //                smtp.Credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");

        //                var send_mail = new MailMessage();
        //                send_mail.IsBodyHtml = true;
        //                send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Yolito Centro ferretero");
        //                send_mail.To.Add(new MailAddress("irojas@yolito.cl"));

        //                send_mail.Bcc.Add(new MailAddress("lpizarro@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("rsandoval@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("ayoliton@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("mfierro@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("jcifuentes@yolito.cl"));
        //                send_mail.Bcc.Add(new MailAddress("tyolitog@yolito.cl"));
        //                send_mail.Subject = "Confirmación de transferencia del Pedido Número: " + "Web_" + id + ", Fecha: " + fecha_exito;
        //                string textBody = string.Empty;
        //                foreach (var pc in ProductosComprados)
        //                {
        //                     textBody = textBody + ("<br/>Nombre del producto : <strong>" + pc.ItemName + " </strong><br />" + "Código del producto: <strong>" + pc.ItemBarCode + " </strong><br />" + "Precio: <strong>" + pc.Precio + "</strong> <br/>" + "Cantidad: <strong>" + pc.Cantidad + "</strong><hr />");
        //                }
        //                send_mail.Body = textBody;
        //                try
        //                {
        //                    smtp.Send(send_mail);
        //                }
        //                catch (Exception p)
        //                {
        //                    ViewBag.error = "[8]" + p.Message;
        //                }

        //            }
        //            else
        //            {
        //                ViewBag.NoIngresa = "No se ingresó el pedido en el sistema";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.errorC = "Carro expiró";
        //    }
        //    return View("ConfirmaTransferencia");
        //}

        //public bool IngresaTransferenciaAx(int id, string rutcliente, string fecha_exito)
        //{
        //    var CbPagadas = from c in db.CabeceraCarro
        //                    where c.NPedido == id && c.EstadoOc == true
        //                    select c;//tomar el email de aca y el rut
        //    var sessionrut = Session["rutuser"];

        //    var CarrosPagados = from prd in db.DetalleCarro
        //                        where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
        //                        select prd;

        //    var totalCompra = CarrosPagados.Select(c => c.Subtotal).Sum();

        //    var OrdenesPagadas = CbPagadas.FirstOrDefault();

        //    var InsertOrdenesPagadas = db.CarroPagado.Create();

        //    if (sessionrut != null)
        //    {
                
        //        InsertOrdenesPagadas.NPedido = OrdenesPagadas.NPedido;
        //        InsertOrdenesPagadas.RutCliente = OrdenesPagadas.RutCliente;
        //        InsertOrdenesPagadas.NombreCliente = OrdenesPagadas.NombreCliente;
                
        //        InsertOrdenesPagadas.CodigoCompraTBK = "Trasferencia";
        //        InsertOrdenesPagadas.NumTarjetaTBK = "transf";
        //        InsertOrdenesPagadas.NCuotasTBK = "transf";
        //        InsertOrdenesPagadas.FechaCompra = fecha_exito;
        //        InsertOrdenesPagadas.Monto = totalCompra.ToString();
        //        InsertOrdenesPagadas.EstadoOc = OrdenesPagadas.EstadoOc;
        //        db.CarroPagado.Add(InsertOrdenesPagadas);

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception pr)
        //        {
        //            ViewBag.error = "[5]" + pr.Message;
        //        }
        //        //ingresar pago en AX

        //        object IngresaCabecera;
        //        object IngresaDetalleCarro;
        //        object ingresaPago;
        //        Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
        //        System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
        //        DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
        //        string[] clie = OrdenesPagadas.RutCliente.Split('-');
        //        string rut = clie[0];
        //        string dig = clie[1];
        //        try
        //        {
        //            //ingresar el pedido.
        //            var getdata = CbPagadas.FirstOrDefault();

        //            IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
        //                "Web_" + OrdenesPagadas.NPedido,
        //                rut + "-" + dig, 
        //                getdata.ComunaDespacho, 
        //                getdata.CalleDespacho, 
        //                getdata.ObservacionDespacho, 
        //                "", 
        //                getdata.ModoEntrega, 
        //                getdata.Local  );

        //            ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay",
        //                "Web_" + OrdenesPagadas.NPedido, 
        //                rut + "-" + dig, 
        //                "", 
        //                "", 
        //                "", 
        //                "", 
        //                OrdenesPagadas.MontoPagado);

        //            foreach (var d in CarrosPagados)
        //            {
        //                IngresaDetalleCarro = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine", 
        //                    "Web_" + d.NPedido,
        //                    d.ItemBarCode, 
        //                    d.Cantidad,
        //                    d.Precio);
        //            }
        //            //enviar monto del flete
        //            //o1 = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight", "Web_" + OrdenesPagadas.NPedido, OrdenesPagadas.CostoDespacho);
        //            DynAx.Logoff();


        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.error = "[6]" + ex.Message;
        //            DynAx.Logoff();
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        InsertOrdenesPagadas.NPedido = OrdenesPagadas.NPedido;
        //        InsertOrdenesPagadas.RutCliente = "1-9";
        //        InsertOrdenesPagadas.NombreCliente = "Cliente no presenta registro";
        //        InsertOrdenesPagadas.CodigoCompraTBK = "Trasferencia";
        //        InsertOrdenesPagadas.NumTarjetaTBK = "transf";
        //        InsertOrdenesPagadas.NCuotasTBK = "transf";
        //        InsertOrdenesPagadas.FechaCompra = fecha_exito;
        //        InsertOrdenesPagadas.Monto = totalCompra.ToString();
        //        InsertOrdenesPagadas.EstadoOc = OrdenesPagadas.EstadoOc;
        //        db.CarroPagado.Add(InsertOrdenesPagadas);

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception pr)
        //        {
        //            ViewBag.error = "[5]" + pr.Message;
        //        }

        //        object IngresaCabecera;
        //        object IngresaDetalleCarro;
        //        object ingresaPago;
        //        object IngresaDespacho;
        //        Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
        //        System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
        //        DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
        //        string[] clie = OrdenesPagadas.RutCliente.Split('-');
        //        string rut = clie[0];
        //        string dig = clie[1];
        //        try
        //        {
        //            //ingresar el pedido.
        //            var getdata = CbPagadas.FirstOrDefault();

        //            if (OrdenesPagadas.ModoEntrega == "D")
        //            {
        //                IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable",
        //                    "Web_" + OrdenesPagadas.NPedido, 
        //                    "1-9", 
        //                    OrdenesPagadas.ComunaDespacho, 
        //                    OrdenesPagadas.CalleDespacho, 
        //                    OrdenesPagadas.ObservacionDespacho, 
        //                    "D", 
        //                    "03");

        //                ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", 
        //                    "Web_" + OrdenesPagadas.NPedido,
        //                    "1-9", 
        //                    "", 
        //                    "", 
        //                    "", 
        //                    "", 
        //                    OrdenesPagadas.MontoPagado);
        //                //enviar monto del flete
        //                IngresaDespacho = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight", 
        //                    "Web_" + OrdenesPagadas.NPedido, 
        //                    OrdenesPagadas.CostoDespacho);
        //            }
        //            else
        //            {
        //                IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
        //                    "Web_" + OrdenesPagadas.NPedido, 
        //                    "1-9", 
        //                    "Retira", 
        //                    "Retira", 
        //                    "Retira en local", 
        //                    "R",
        //                    OrdenesPagadas.Local);

        //                ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", "Web_" + OrdenesPagadas.NPedido, 
        //                    "1-9",
        //                    "", 
        //                    "", 
        //                    "", 
        //                    "", 
        //                    OrdenesPagadas.MontoPagado);
        //            }

                    

        //            foreach (var d in CarrosPagados)
        //            {
        //                IngresaDetalleCarro = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine", 
        //                    "Web_" + d.NPedido, 
        //                    d.ItemBarCode, 
        //                    d.Cantidad,
        //                    d.Precio);
        //            }
                    
        //            DynAx.Logoff();


        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.error = "[6]" + ex.Message;
        //            DynAx.Logoff();
        //            return false;
        //        }

        //    }
            
        //}
        public bool CrearPedidoAx(int numero_oc, string comunalocal, string ModoEntrega)
        {

            object CreateSalesTable;
            object CreateSalesLine;
            object CreateSalesWebpay;
            object CreateSalesLineFreight;

            var CarroPagado = (from c in db.CabeceraCarro
                               where c.NPedido == numero_oc && c.EstadoOc == true
                               select c);

            var ProductosComprados = (from prd in db.DetalleCarro
                                      where prd.NPedido == numero_oc && prd.CabeceraCarro.EstadoOc == true
                                      select prd).ToList();

            var OrdenesPagadas = CarroPagado.FirstOrDefault();

            var CarroPagado1 = (from prd in db.CarroPagado
                                where prd.NPedido == numero_oc
                                select prd).FirstOrDefault();

            if (CarroPagado.Count() != 0)
            {

                try
                {
                    System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                    DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                    if (Session["rutuser"] == null)
                    {
                        CreateSalesTable = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable",
                              "AWeb_" + OrdenesPagadas.NPedido,//SalesId
                              "1-9",//CustAccount
                              comunalocal,//_County
                              ModoEntrega,//_Street
                              ModoEntrega,//Obs
                              OrdenesPagadas.ResponsableDespacho,//Contacto
                              OrdenesPagadas.ModoEntrega,//TDS
                              OrdenesPagadas.Local,//InventLocationId
                              OrdenesPagadas.tipoDocumento);//SalesDocumentCode
                    }
                    else
                    {
                        CreateSalesTable = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable",
                               "AWeb_" + OrdenesPagadas.NPedido,//SalesId
                               OrdenesPagadas.RutCliente,//CustAccount
                               comunalocal,//_County
                               ModoEntrega,//_Street
                               ModoEntrega,//Obs
                               OrdenesPagadas.ResponsableDespacho,//Contacto
                               OrdenesPagadas.ModoEntrega,//TDS
                               OrdenesPagadas.Local,//InventLocationId
                               OrdenesPagadas.tipoDocumento);//SalesDocumentCode
                    }

                    foreach (var p in ProductosComprados)
                    {

                        CreateSalesLine = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine",
                            "AWeb_" + p.NPedido,//SalesId
                            p.ItemBarCode,//ItemBarCode
                            p.Cantidad,//Qty
                            p.Precio);//Price

                    }

                    //agregar el costo del despacho
                    if (ModoEntrega == "D")
                    {
                        CreateSalesLineFreight = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight",
                    "AWeb_" + OrdenesPagadas.NPedido,//SalesId
                    OrdenesPagadas.CostoDespacho);//Freight
                    }


                    CreateSalesWebpay = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay",
                        "AWeb_" + CarroPagado1.NPedido,//SalesId
                        CarroPagado1.RutCliente,//CustAccount
                        CarroPagado1.CodigoCompraTBK,//CodTrans
                        CarroPagado1.NumTarjetaTBK,//CardNum
                        CarroPagado1.TipoCuota,//PayMode
                        CarroPagado1.NCuotasTBK,//amount Feed
                        CarroPagado1.Monto//amount
                        );
                    DynAx.Logoff();
                    return true;

                }
                catch (Exception e)
                {

                    TempData["ErrorCreateSalesLine"] = e.Message;
                    return false;
                }


            }
            return false;

        }
        public bool ChangeEstado_oc(int oc)
        {
            var query = (from c in db.CabeceraCarro
                         where c.NPedido == oc
                         select c).FirstOrDefault();
            query.EstadoOc = true;
            try
            {
                db.SaveChanges();
                return true;

            }
            catch (Exception p)
            {
                TempData["ChangeEstado"] = "[9]" + p.Message;
                return false;
            }

        }
        public ActionResult ConfirmaTransferencia(int id)
        {
            //cambiar estado de la compra a 1 
            var CarroPagado = (from c in db.CarroPagado
                               where c.NPedido == id
                               select c).FirstOrDefault();

            if (Session["cart"] != null)
            {
                if (ChangeEstado_oc(id) == true)
                {
                    var OrdenesPagadas = (from c in db.CabeceraCarro
                                          where c.NPedido == id && c.EstadoOc == true
                                          select c).FirstOrDefault();

                    var totalCompra = (from d in db.DetalleCarro
                                       where d.NPedido == id && d.CabeceraCarro.EstadoOc == true
                                       select d.Subtotal).Sum();

                    var ProductosComprados = from prd in db.DetalleCarro
                                             where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
                                             select prd;


                    DateTime hoydia = DateTime.Today;
                    string fecha = hoydia.ToString("MMdd");
                    string fecha_exito = hoydia.ToString("dd/MM/yyyy");

                    var InsertOrdenesPagadas = db.CarroPagado.Create();

                    InsertOrdenesPagadas.NPedido = OrdenesPagadas.NPedido;
                    InsertOrdenesPagadas.RutCliente = OrdenesPagadas.RutCliente;
                    InsertOrdenesPagadas.NombreCliente = getNombreCliente(id);
                    InsertOrdenesPagadas.CodigoCompraTBK = "Trasferencia";
                    InsertOrdenesPagadas.NumTarjetaTBK = "transf";
                    InsertOrdenesPagadas.NCuotasTBK = "transf";
                    InsertOrdenesPagadas.TipoCuota = "transf";
                    InsertOrdenesPagadas.FechaCompra = fecha_exito;
                    InsertOrdenesPagadas.Monto = totalCompra.ToString();
                    InsertOrdenesPagadas.EstadoOc = OrdenesPagadas.EstadoOc;
                    InsertOrdenesPagadas.EmailCliente = OrdenesPagadas.EmailCliente;
                    InsertOrdenesPagadas.tipoDocumento = OrdenesPagadas.tipoDocumento;
                    db.CarroPagado.Add(InsertOrdenesPagadas);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception pr)
                    {
                        ViewBag.error = "[5]" + pr.Message;
                    }

                    string comunalocal = string.Empty;
                    string modoEntrega = string.Empty;
                    switch (OrdenesPagadas.Local)
                    {
                        case "01":
                            comunalocal = "Las Condes";
                            break;

                        case "02":
                            comunalocal = "Colina";
                            break;

                        case "03":
                            comunalocal = "Quilicura";
                            break;
                    }
                    switch (OrdenesPagadas.ModoEntrega)
                    {
                        case "D":
                            modoEntrega = "Despacho";
                            break;
                        case "R":
                            modoEntrega = "Retira en tienda";
                            break;
                    }
                    if (CrearPedidoAx(id, comunalocal, modoEntrega) == true)
                    {
                        //enviar mails
                        ViewBag.ExitoT = "1";
                        EmailDetalleCliente(id,fecha_exito,modoEntrega);
                        EmailPago(id,fecha_exito);
                    }
                    else
                    {
                        ViewBag.error = "No se ingreso el pedido en el sistema";
                    }

                }
                else
                {
                    ViewBag.error = "No se actualizo el estado del pedido";
                }
               
                
            }
            else
            {
                ViewBag.error = "Carrito expiró";
                return View();
            }
            

            return View();
        }

        public string getNombreCliente(int TBKORDEN)
        {
            var OrdenesPagadas1 = (from c in db.CabeceraCarro
                                   where c.NPedido == TBKORDEN && c.EstadoOc == true
                                   select c).FirstOrDefault();
            if (Session["rutuser"] != null)
            {
                string textBody = OrdenesPagadas1.NombreCliente;
                return textBody;
            }
            else
            {
                string textBody = OrdenesPagadas1.ResponsableDespacho;
                return textBody;
            }

        }

        public void EmailDetalleCliente(int id, string fecha_exito, string ModoEntrega)
        {
            //enviar mail a luis pizarro
            var getCarroPagado = (from pr in db.CarroPagado
                                  where pr.NPedido == id
                                  select pr).FirstOrDefault();
            var ProductosComprados = (from pr in db.DetalleCarro
                                      where pr.NPedido == id && pr.CabeceraCarro.EstadoOc == true
                                      select pr).ToList();
            //conexion smtp
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Timeout = 10000;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");
            //creando el mail

            var send_mail = new MailMessage();
            send_mail.Subject = "Yolito Centro Ferretero, Orden Número: AWeb_" + id + ", Fecha: " + fecha_exito;
            send_mail.IsBodyHtml = true;
            send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Ventas Yolito");
            send_mail.To.Add(new MailAddress(getCarroPagado.EmailCliente));
            send_mail.Bcc.Add(new MailAddress("lpizarro@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("rsandoval@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("ayoliton@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("mfierro@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("tyolitog@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("jcifuentes@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("jmarin@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("hgarza@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("rsanmartin@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("myolito@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("irojas@yolito.cl"));
            send_mail.BodyEncoding = System.Text.Encoding.UTF8;

            string textBody = "<h1>Comprobante de venta internet AWeb_" + id + "</h1></br>Hola <strong>" + getNombreCliente(id) + "</strong><br />" +
"<br /> El detalle de tu compra es: <br /><table ><tr><td>Últimos 4 dígitos de la tarjeta:</td> <td>" + getCarroPagado.NumTarjetaTBK + "</td></tr><tr><td>Fecha de pago:</td><td>" + fecha_exito + "</td></tr><tr><td>Numero Orden de compra:</td><td>" + id + "</td></tr><tr><td>Monto:</td><td>" + getCarroPagado.Monto + "</td></tr><tr><td>Codigo de autorización:</td><td>" + getCarroPagado.CodigoCompraTBK + "</td></tr><tr><td>Numero de cuotas seleccionadas:</td><td>" + getCarroPagado.NCuotasTBK + "</td></tr><tr><td>Tipo Cuota seleccionada: </td><td>" + getCarroPagado.TipoCuota + "</td></tr><tr><td>Tipo documento:</td><td>" + getCarroPagado.tipoDocumento + "</td></tr><tr><td>Modo de entrega: </td><td>" + ModoEntrega + "</td></tr></table> <br /> Detalle de los productos comprados: <br/>";
            string textbody1 = string.Empty;
            string tableProductosComprados = "<table><tr ><th>Productos comprados</th><th>Código del producto</th><th>Cantidad</th><th>Precio</th></tr>";
            foreach (var pc in ProductosComprados)
            {
                textbody1 = textBody + tableProductosComprados + "<tr><td>" + pc.ItemName + "</td><td>" + pc.ItemBarCode + "</td><td>" + pc.Cantidad + "</td><td> $ " + pc.Precio.ToString("#,##") + "</td></tr></table></br><h3>¡Gracias por preferir nuestra ferreteria! </h3>";
            }

            send_mail.Body = textbody1;
            try
            {
                smtp.Send(send_mail);
            }
            catch (Exception lp)
            {
                ViewBag.error = lp.Message;
            }
        }

        public void EmailPago(int id, string fecha_exito)
        {
            //enviar mail a luis pizarro
            var getCarroPagado = (from pr in db.CarroPagado
                                  where pr.NPedido == id
                                  select pr).FirstOrDefault();


            //conexion smtp
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Timeout = 10000;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");
            //creando el mail

            var send_mail = new MailMessage();
            send_mail.Subject = "Yolito Centro Ferretero, Orden Número: AWeb_" + id + ", Fecha: " + fecha_exito;
            send_mail.IsBodyHtml = true;
            send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Ventas Yolito");
            send_mail.To.Add(new MailAddress("lpizarro@yolito.cl"));
            send_mail.BodyEncoding = System.Text.Encoding.UTF8;
            send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));

            string textBody = "<h1>Comprobante de venta internet AWeb_" + id + "</h1></br><h4>Datos del cliente para facturación:</h4></br><table><tr><td>Rut del cliente:</td><td>" + getCarroPagado.RutCliente + "</td></tr><tr><td>Nombre del cliente:</td><td>" + getCarroPagado.NombreCliente + "</td></tr><tr><td>Código compra: </td><td>" + getCarroPagado.CodigoCompraTBK + "</td></tr><tr><td>Número de tarjeta:</td><td>" + getCarroPagado.NumTarjetaTBK + "</td></tr><tr><td>Numero de cuotas:</td><td>" + getCarroPagado.NCuotasTBK + "</td></tr><tr><td>Fecha de la compra:</td><td>" + getCarroPagado.FechaCompra + "</td></tr><tr><td>Monto:</td><td>" + getCarroPagado.Monto + "</td></tr><tr><td>Tipo de cuota:</td><td>" + getCarroPagado.TipoCuota + "</td></tr><tr><td>Email del cliente:</td><td>" + getCarroPagado.EmailCliente + "</td></tr></table>";

            send_mail.Body = textBody;
            try
            {
                smtp.Send(send_mail);
                //count = count + 1;
                //ViewBag.pase = count;
            }
            catch (Exception lp)
            {
                ViewBag.error = lp.Message;
            }

        }
    }
}
