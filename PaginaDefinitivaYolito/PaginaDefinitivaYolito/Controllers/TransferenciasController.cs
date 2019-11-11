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
    public class TransferenciasController : Controller
    {
        
        private PaginaWebEntities1 db = new PaginaWebEntities1();
        
        public ActionResult Index()
        {
            return View();
        }

        public bool PagaOrden(int id)
        {
            
            var CbPagadas = (from c in db.CabeceraCarro
                            where c.NPedido == id && c.EstadoOc == false
                            select c).FirstOrDefault();//tomar el email de aca y el rut

            CbPagadas.EstadoOc = true;
            db.SaveChanges();
            Session["cart"] = null;
            return true;
            
        }


        public ActionResult ConfirmaTransferencia(int id)
        {
            //ingresar transferencia AX y enviar correos. validar si es retira
            var CbPagadas = from c in db.CabeceraCarro
                            where c.NPedido == id && c.EstadoOc == true
                            select c;//tomar el email de aca y el rut

            var getdata = CbPagadas.FirstOrDefault();
            ViewBag.npedido = id;
            
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

            object IngresaCabecera;
            object IngresaPago;
            object MontoDespacho;
            object IngresaDetalleCarro;

            if (PagaOrden(id) == true)
            {
                //ingresar cabecera en AX
                foreach (var it in CbPagadas)
                {
                    if (it.ModoEntrega == "D")
                    {
                        try
                        {
                            IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                                "AWeb_" + getdata.NPedido, 
                                "1-9", 
                                getdata.ComunaDespacho, 
                                getdata.CalleDespacho, 
                                getdata.ObservacionDespacho, 
                                "D", 
                                "03");
                        }
                        catch (Exception ep)
                        {
                            ViewBag.error = "[15] " + ep.Message;
                        }


                        try
                        {
                            IngresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay",
                                "AWeb_" + getdata.NPedido, 
                                "1-9", 
                                "", 
                                "", 
                                "", 
                                "",
                                getdata.MontoPagado);

                        }
                        catch (Exception jp)
                        {
                            ViewBag.error = "[16] " + jp.Message;
                        }

                        try
                        {
                            MontoDespacho = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight",
                                "AWeb_" + getdata.NPedido, 
                                getdata.CostoDespacho);
                        }
                        catch (Exception lp)
                        {
                            ViewBag.error = "[17]" + lp.Message;
                        }

                        try
                        {
                            var CarrosPagados = from prd in db.DetalleCarro
                                                where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
                                                select prd;

                            foreach (var d in CarrosPagados)
                            {
                                IngresaDetalleCarro = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine", 
                                    "AWeb_" + d.NPedido, 
                                    d.ItemBarCode, 
                                    d.Cantidad, 
                                    d.Precio);
                            }
                        }
                        catch (Exception op)
                        {
                            ViewBag.error = "[18]"+op.Message;
                        }

                        ViewBag.ExitoT = "exito";
                    }
                    else
                    {
                        try
                        {
                            if (Session["rutuser"] != null)
                            {
                                //si se registra enviar el rut
                                IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                                  "AWeb_" + getdata.NPedido, 
                                  Session["rutuser"].ToString(),
                                  getdata.ComunaDespacho, 
                                  getdata.CalleDespacho, 
                                  getdata.ObservacionDespacho, 
                                  "R", 
                                  getdata.Local);
                            }
                            else
                            {
                                //si no es registro. el pedido se registra con rut-9
                                IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                                    "AWeb_" + getdata.NPedido, 
                                    "1-9", 
                                    getdata.ComunaDespacho,
                                    getdata.CalleDespacho,
                                    getdata.ObservacionDespacho, 
                                    "R", 
                                    getdata.Local);
                            }

                        }
                        catch (Exception ep)
                        {
                            ViewBag.error = "[15] " + ep.Message;
                        }

                        try
                        {
                            if (Session["rutuser"] != null)
                            {
                                IngresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay",
                                    "AWeb_" + getdata.NPedido, 
                                    Session["rutuser"].ToString(), 
                                    "", 
                                    "", 
                                    "",
                                    "", 
                                    getdata.MontoPagado);
                            }
                            else
                            {
                                IngresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", 
                                    "AWeb_" + getdata.NPedido, 
                                    "1-9",
                                    "", 
                                    "", 
                                    "",
                                    "", 
                                    getdata.MontoPagado);
                            }


                        }
                        catch (Exception jp)
                        {
                            ViewBag.error = "[16] " + jp.Message;
                        }

                        //try
                        //{
                        //    MontoDespacho = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight", "Web_" + getdata.NPedido, getdata.CostoDespacho);
                        //}
                        //catch (Exception lp)
                        //{
                        //    ViewBag.error = "[17]" + lp.Message;
                        //}

                        try
                        {
                            var CarrosPagados = from prd in db.DetalleCarro
                                                where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
                                                select prd;

                            foreach (var d in CarrosPagados)
                            {
                                IngresaDetalleCarro = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine", 
                                    "AWeb_" + d.NPedido, 
                                    d.ItemBarCode, 
                                    d.Cantidad,
                                    d.Precio);
                            }
                        }
                        catch (Exception op)
                        {
                            ViewBag.error = "[18]"+op.Message;
                        }

                        ViewBag.ExitoT = "exito";
                    }
                }
                

            }
            DynAx.Logoff();//cierro session ax

            return View();
        }
        public ActionResult TransferenciaSinRegistro(int id)
        {
            //ingresar transferencia AX y enviar correos. validar si es retira
            var CbPagadas = from c in db.CabeceraCarro
                            where c.NPedido == id && c.EstadoOc == true
                            select c;//tomar el email de aca y el rut

            var getdata = CbPagadas.FirstOrDefault();

            DateTime hoydia = DateTime.Today;
            string fecha = hoydia.ToString("MMdd");
            string fecha_exito = hoydia.ToString("dd/MM/yyyy");

            if (PagaOrden(id) == true)
            {
                ViewBag.npedido = id;
                object IngresaCabecera;
                object IngresaDetalleCarro;
                object ingresaPago;
                object MontoDespacho;
                var CarrosPagados = from prd in db.DetalleCarro
                                    where prd.NPedido == id && prd.CabeceraCarro.EstadoOc == true
                                    select prd;

                int montototal = 0;
                
                
                string textBody = string.Empty;
                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
                try
                {
                    //ingresar el pedido.
                    IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                        "AWeb_" + getdata.NPedido, 
                        "1-9", getdata.ComunaDespacho, 
                        getdata.CalleDespacho, 
                        getdata.ObservacionDespacho, 
                        "D", 
                        "03");

                    ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", 
                        "AWeb_" + getdata.NPedido, 
                        "1-9",
                        "", 
                        "", 
                        "", 
                        "", getdata.MontoPagado);

                    MontoDespacho = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight", 
                        "AWeb_" + getdata.NPedido, 
                        getdata.CostoDespacho);

                    if (getdata.ModoEntrega == "D")
                    {
                        int valordesp = Convert.ToInt32(getdata.CostoDespacho);
                        int valorpagado = Convert.ToInt32(getdata.MontoPagado);

                        montototal = valordesp + valorpagado;

                        IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                            "AWeb_" + getdata.NPedido, 
                            "1-9", 
                            getdata.ComunaDespacho, 
                            getdata.CalleDespacho, 
                            getdata.ObservacionDespacho, 
                            "D", 
                            "03");

                        ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", 
                            "AWeb_" + getdata.NPedido, 
                            "1-9", 
                            "", 
                            "", 
                            "", 
                            "", 
                            getdata.MontoPagado);

                        MontoDespacho = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLineFreight", 
                            "AWeb_" + getdata.NPedido, 
                            getdata.CostoDespacho);

                        textBody = "Hola estimado, <strong>" + getdata.NombreCliente + "</strong><br />" +
    "<br /> El detalle de tu compra es: <br /><table ><tr><td>Pedido:</td> <td>" + "Web_" + id + "</td></tr><tr><td>Fecha de pago:</td><td>" + fecha_exito + "</td></tr><tr><td>Tipo Pago:</td> <td>Transferencia</td></tr><tr><td>Modo entrega: </td><td>" + "Despacho" + "</td></tr><tr><td>Monto total:</td><td>" + montototal.ToString("#,##") + "</td></tr></table> <br /> Detalle de los productos comprados: <br/>";
                    }

                    else
                    {
                        IngresaCabecera = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesTable", 
                            "AWeb_" + getdata.NPedido, 
                            "1-9", 
                            "Retira", 
                            "Retira", 
                            "Retira en local",
                            "R", 
                            getdata.Local);

                        ingresaPago = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesWebpay", 
                            "AWeb_" + getdata.NPedido, 
                            "1-9", 
                            "", 
                            "", 
                            "", 
                            "", 
                            getdata.MontoPagado);
                       
                        textBody = "Hola estimado, <strong>" + getdata.NombreCliente + "</strong><br />" +
    "<br /> El detalle de tu compra es: <br /><table ><tr><td>Pedido pagado:</td> <td>" + "Web_" + id + "</td></tr><tr><td>Fecha de pago:</td><td>" + fecha_exito + "</td></tr><tr><td>Tipo Pago:</td> <td>Transferencia</td></tr><tr><td>Modo Entrega:</td><td>" + getdata.ModoEntrega + "</td></tr><tr><td>Local :</td><td>" + getdata.Local + "</td></tr><tr><td>Monto total:</td><td>" + getdata.MontoPagado + "</td></tr></table> <br /> Detalle de los productos comprados: <br/>";

                        
                    }
                    foreach (var d in CarrosPagados)
                    {
                        IngresaDetalleCarro = DynAx.CallStaticClassMethod("WebPageConection", "CreateSalesLine", 
                            "AWeb_" + d.NPedido,
                            d.ItemBarCode, 
                            d.Cantidad, 
                            d.Precio);
                    }
                    
                    DynAx.Logoff();

                }
                catch (Exception ex)
                {
                    ViewBag.error = "[6]" + ex.Message;
                    DynAx.Logoff();

                }
                var ProductosComprados = from pr in db.DetalleCarro
                                         where pr.NPedido == id && pr.CabeceraCarro.EstadoOc == true
                                         select pr;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");

                var send_mail = new MailMessage();
                send_mail.IsBodyHtml = true;
                send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Yolito Centro ferretero");
                send_mail.To.Add(new MailAddress("irojas@yolito.cl"));

                send_mail.Bcc.Add(new MailAddress("lpizarro@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("rsandoval@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("ayoliton@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("mfierro@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("yparra@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("jcifuentes@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("tyolitog@yolito.cl"));
                send_mail.Subject = "Confirmación de transferencia del Pedido Número: " + "AWeb_" + id + ", Fecha: " + fecha_exito;
                foreach (var pc in ProductosComprados)
                {
                    textBody = textBody + ("<br/>Nombre del producto : <strong>" + pc.ItemName + " </strong><br />" + "Código del producto: <strong>" + pc.ItemBarCode + " </strong><br />" + "Precio: <strong>" + pc.Precio + "</strong> <br/>" + "Cantidad: <strong>" + pc.Cantidad + "</strong><hr />");
                }
                send_mail.Body = textBody;
                try
                {
                    smtp.Send(send_mail);
                }
                catch (Exception p)
                {
                    ViewBag.error = "[8]" + p.Message;
                }

            }
            else
            {
                ViewBag.error = "No se actualizo el estado del pedido";
            }
            
            return View();
        }

        



    }
}
