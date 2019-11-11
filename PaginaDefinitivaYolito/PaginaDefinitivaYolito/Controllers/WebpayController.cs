using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.IO;
using PaginaDefinitivaYolito.Models;
using System.Diagnostics;
using System.Net.Mail;

namespace PaginaDefinitivaYolito.Controllers
{
    public class WebpayController : Controller
    {
        //
        // GET: /Webpay/
        private PaginaWebEntities1 db = new PaginaWebEntities1();

        private Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
        

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Pago_webpay()
        {
            return View(db.DetalleCarro);
        }

        public ActionResult Exito1()
        {
            //respuesta desde CGI
            string TBK_ORDEN_COMPRA = Request.Form["TBK_ORDEN_COMPRA"];
            string TBK_MONTO = Request.Form["TBK_MONTO"];
            string TBK_ID_SESION = Request.Form["TBK_ID_SESION"];
            string TBK_HORA_TRANSACCION = Request.Form["TBK_HORA_TRANSACCION"];
            ///////////////////////////////////
            ViewBag.oc = TBK_ORDEN_COMPRA;
            ViewBag.monto = TBK_MONTO;
            ViewBag.user = TBK_ID_SESION;
            //almaceno las variables para pasarlas a la vista

            //fecha de hoy
            DateTime hoydia = DateTime.Today;
            string fecha = hoydia.ToString("MMdd");
            string fecha_exito = hoydia.ToString("dd/MM/yyyy");
            ///////////////////////////

            //get log de respuesta de CGI
            string[] lector = System.IO.File.ReadAllLines(Server.MapPath("~/cgi-bin/log/tbk_bitacora_TR_NORMAL_" + fecha + ".log"));
            //string[] lector = System.IO.File.ReadAllLines(Server.MapPath("~/cgi-bin/log/tbk_bitacora_TR_NORMAL_0105.log"));
            string ultimoReg = lector[lector.Length - 1]; 
            string[] separador = ultimoReg.Split(';');
            //rescato siempre el ultimo registro

            string[] tarjeta = separador[7].Split('=');
            ViewBag.numTarjeta = tarjeta[1]; // tarjeta
            string[] fecha1 = separador[9].Split('=');
            ViewBag.fecha = fecha_exito; // fecha de la compra
            string[] usuario = separador[11].Split('=');
            string elusuario = usuario[1];
            ViewBag.user = usuario[1];
            string[] tipotranx = separador[3].Split('=');
            ViewBag.tipotranx = tipotranx[1]; //tipo transaccion
            string[] monto = separador[5].Split('=');
            ViewBag.monto = ((Convert.ToInt32(monto[1])) / 100).ToString("#,##");//monto
            string[] codigo = separador[6].Split('=');
            ViewBag.codigo = codigo[1];// codigo autoriza
            string[] hora = separador[10].Split('=');
            string lahora = hora[1];
            ViewBag.hora = lahora;
            int hora1 = Convert.ToInt32(lahora.Substring(0, 2));
            int minutos = Convert.ToInt32(lahora.Substring(2, 2));
            ViewBag.minutos = minutos;
            int segundos = Convert.ToInt32(lahora.Substring(4, 2));
            ViewBag.HoraTrx = hora1 + ":" + minutos + ":" + segundos;
            //ViewBag.id_trans =separador[12].Substring(1, separador[12].IndexOf('='));
            string[] cuotas = separador[14].Split('=');
            string lascuotas = cuotas[1];
            ViewBag.lascuotas = lascuotas;
            string[] tipoPago = separador[12].Split('=');
            string eltipoPago = tipoPago[1];
            string tipotrx = tipotranx[1];
            string[] tipoCuota = separador[13].Split('=');
            string tipopago = tipoCuota[1];
            ViewBag.cuotas = cuotas[1];

            ViewBag.tipocuota = tipopago;

            string elcodigo = codigo[1];
            string latarjeta = tarjeta[1];
            int elmonto = ((Convert.ToInt32(monto[1])) / 100);//monto
            string elmontopagado = elmonto.ToString();
            string num_cuotas = cuotas[1];
            //transformo la OC en int. esta se utiliza
            int numero_oc = Convert.ToInt32(TBK_ORDEN_COMPRA);
            ////////////////
            
            string comunalocal = string.Empty;
            string modoEntrega = string.Empty;
            if (ChangeEstado_oc(numero_oc) == true)
            {
                var OrdenesPagadas = (from c in db.CabeceraCarro
                                      where c.NPedido == numero_oc && c.EstadoOc == true
                                      select c).FirstOrDefault();

                if (IngresaPagoBD(numero_oc, elcodigo, latarjeta, num_cuotas, fecha_exito, elmontopagado, tipopago, OrdenesPagadas.tipoDocumento) == true)
                {
                    //logueo en sistema
                    var CarroPagado = (from c in db.CarroPagado
                                       where c.NPedido.ToString() == TBK_ORDEN_COMPRA
                                       select c).FirstOrDefault();

                    var ProductosComprados = (from prd in db.DetalleCarro
                                             where prd.NPedido == numero_oc && prd.CabeceraCarro.EstadoOc == true
                                             select prd).ToList();

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

                    if(CrearPedidoAx(numero_oc,comunalocal,modoEntrega) == true){
                        
                        EmailPago(numero_oc, fecha_exito);
                        EmailDetalleCliente(numero_oc, fecha_exito, modoEntrega);
                        return View("Exito");
                    }
                    else
                    {
                        ViewBag.errorPago = "No se ingreso el pago en el sistema, favor comunicate con nuestro vendedor.";
                    }

                    
                }
                else
                {
                    ViewBag.errorPago = "Error al ingresar el pago";
                }

            }
           

            return View("Exito");
        }

        public void sendEmailComprobantePago(string TBK_ORDEN_COMPRA,string fecha_exito, string textBody, string emailCliente)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Timeout = 10000;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");
            var send_mail = new MailMessage();

            send_mail.IsBodyHtml = true;
            send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Ventas web Yolito");
            send_mail.To.Add(new MailAddress(emailCliente));
            send_mail.BodyEncoding = System.Text.Encoding.UTF8;

            send_mail.Bcc.Add(new MailAddress("lpizarro@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("rsandoval@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("ayoliton@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("mfierro@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("tyolitog@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("jcifuentes@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("hgarza@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("rsanmartin@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("myolito@yolito.cl"));
            send_mail.Bcc.Add(new MailAddress("irojas@yolito.cl"));
            send_mail.Subject = "Yolito Centro Ferretero, Orden Número: AWeb_" + TBK_ORDEN_COMPRA + ", Fecha: " + fecha_exito;

            send_mail.Body = textBody;
            try
            {
                smtp.Send(send_mail);
            }
            catch (Exception lp)
            {
                ViewBag.error = lp.Message;
            }
            
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

        public bool IngresaPagoBD(int oc, string elcodigo, string latarjeta, string num_cuotas, string fecha_exito, string elmontopagado, string tipopago, string tipoDocumento)
        {

            var OrdenesPagadas = (from c in db.CabeceraCarro
                                  where c.NPedido == oc && c.EstadoOc == true
                                  select c).FirstOrDefault();

            var InsertOrdenesPagadas = db.CarroPagado.Create();

            InsertOrdenesPagadas.NPedido = OrdenesPagadas.NPedido;


            if (Session["rutuser"] != null)
            {
                InsertOrdenesPagadas.RutCliente = OrdenesPagadas.RutCliente;
                
            }
            else
            {
                InsertOrdenesPagadas.RutCliente = "1-9";
                
            }
            if (OrdenesPagadas.NombreCliente != null)
            {
                InsertOrdenesPagadas.NombreCliente = OrdenesPagadas.NombreCliente;
            }
            else
            {
                InsertOrdenesPagadas.NombreCliente = OrdenesPagadas.ResponsableDespacho;
            }
            
            InsertOrdenesPagadas.CodigoCompraTBK = elcodigo;
            InsertOrdenesPagadas.NumTarjetaTBK = latarjeta;
            InsertOrdenesPagadas.NCuotasTBK = num_cuotas;
            InsertOrdenesPagadas.FechaCompra = fecha_exito;
            InsertOrdenesPagadas.Monto = elmontopagado;
            InsertOrdenesPagadas.EstadoOc = OrdenesPagadas.EstadoOc;
            InsertOrdenesPagadas.tipoDocumento = tipoDocumento;

            switch (tipopago)
            {
                case "VD":
                    InsertOrdenesPagadas.TipoCuota = "DEBITO";
                    break;

                case "VN":
                    InsertOrdenesPagadas.TipoCuota = "SIN CUOTAS";
                    break;

                case "VC":
                    InsertOrdenesPagadas.TipoCuota = "CUOTAS NORMALES";
                    break;

                case "SI":
                    InsertOrdenesPagadas.TipoCuota = "SIN INTERES";
                    break;

                case "CI":
                    InsertOrdenesPagadas.TipoCuota = "CUOTAS COMERCIO";
                    break;

                case "":
                    InsertOrdenesPagadas.TipoCuota = "NO INFORMADO";
                    break;
            }

            InsertOrdenesPagadas.EmailCliente = OrdenesPagadas.EmailCliente;
            db.CarroPagado.Add(InsertOrdenesPagadas);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {   
                return false;
            }

            
        }

        public ActionResult PagoNoIngresado(int id)
        {
            var CarroPagado = (from c in db.CabeceraCarro
                               where c.NPedido == id && c.EstadoOc == true
                               select c).FirstOrDefault();

            string comunalocal = string.Empty;
            string modoEntrega = string.Empty;
            switch (CarroPagado.Local)
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
            switch (CarroPagado.ModoEntrega)
            {
                case "D":
                    modoEntrega = "Despacho";
                    break;
                case "R":
                    modoEntrega = "Retira en tienda";
                    break;
            }
            if (CrearPedidoAx(id, comunalocal, CarroPagado.ModoEntrega) == true)
            {
                DateTime hoydia = DateTime.Today;
            string fecha = hoydia.ToString("MMdd");
            string fecha_exito = hoydia.ToString("dd/MM/yyyy");
                EmailPago(id,fecha_exito);

                EmailDetalleCliente(id, fecha_exito, modoEntrega);
                ViewBag.PedidoAgregado = "Pedido ingresado correctamente";

                return View();
            }
            else
            {
                ViewBag.error = "No se ingreso el pago";
            }
            return View();
        }

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

                }catch(Exception e){

                    TempData["ErrorCreateSalesLine"] = e.Message;
                    return false;
                }
               
                
            }
            return false;
            
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
"<br /> El detalle de tu compra es: <br /><table ><tr><td>Últimos 4 dígitos de la tarjeta:</td> <td>" + getCarroPagado.NumTarjetaTBK + "</td></tr><tr><td>Fecha de pago:</td><td>" + fecha_exito + "</td></tr><tr><td>Numero Orden de compra:</td><td>" + id + "</td></tr><tr><td>Monto:</td><td>" + getCarroPagado.Monto + "</td></tr><tr><td>Codigo de autorización:</td><td>" + getCarroPagado.CodigoCompraTBK + "</td></tr><tr><td>Numero de cuotas seleccionadas:</td><td>" + getCarroPagado.NCuotasTBK + "</td></tr><tr><td>Tipo Cuota seleccionada: </td><td>" + getCarroPagado.TipoCuota + "</td></tr><tr><td>Tipo documento:</td><td>"+getCarroPagado.tipoDocumento+"</td></tr><tr><td>Modo de entrega: </td><td>" + ModoEntrega + "</td></tr></table> <br /> Detalle de los productos comprados: <br/>";
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

            string textBody = "<h1>Comprobante de venta internet AWeb_" + id + "</h1></br><h4>Datos del cliente para facturación:</h4></br><table><tr><td>Rut del cliente:</td><td>" + getCarroPagado.RutCliente + "</td></tr><tr><td>Nombre del cliente:</td><td>" + getCarroPagado.NombreCliente + "</td></tr><tr><td>Código compra: </td><td>" + getCarroPagado.CodigoCompraTBK + "</td></tr><tr><td>Número de tarjeta:</td><td>" + getCarroPagado.NumTarjetaTBK + "</td></tr><tr><td>Numero de cuotas:</td><td>" + getCarroPagado.NCuotasTBK + "</td></tr><tr><td>Fecha de la compra:</td><td>" + getCarroPagado.FechaCompra + "</td></tr><tr><td>Monto:</td><td>" + getCarroPagado.Monto + "</td></tr><tr><td>Tipo de cuota:</td><td>" + getCarroPagado.TipoCuota + "</td></tr><tr><td>Email del cliente:</td><td>"+getCarroPagado.EmailCliente+"</td></tr></table>";

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

        public ActionResult Fracaso()
        {
            string TBK_ORDEN_COMPRA = Request.Form["TBK_ORDEN_COMPRA"];
            string monto = Request.Form["TBK_MONTO"];

            ViewBag.Monto = monto;
            ViewBag.OrdenFracasada = TBK_ORDEN_COMPRA;
            return View();
        }

        public string Xt_Cierre()
        {

            //return "ACEPTADO";
            string TBK_MONTO = Request.Form["TBK_MONTO"];
            string TBK_ORDEN_COMPRA = Request.Form["TBK_ORDEN_COMPRA"];
           

            int elmonto = (Convert.ToInt32(TBK_MONTO) / 100);

            //var verifica_monto = (from ord in db.DetalleCarro
            //                      where ord.NPedido.ToString() == TBK_ORDEN_COMPRA && ord.CabeceraCarro.EstadoOc == false
            //                      select ord.Subtotal).Sum();

            var verifica_orden = (from cabecera in db.CabeceraCarro
                                  where cabecera.NPedido.ToString() == TBK_ORDEN_COMPRA
                                  && cabecera.EstadoOc == false
                                  select cabecera).FirstOrDefault();
            int montopagado = Convert.ToInt32(verifica_orden.MontoPagado);
            
            //if (verifica_monto == 1375)
            //{
            //    return "ACEPTADO" + verifica_orden.EstadoOc + "orden";
            //}
            //else
            //{
            //    return "RECHAZADO";
            //}
            if (montopagado == elmonto)
            {
                if (verifica_orden.EstadoOc == false)
                {
                    return "ACEPTADO";

                }
                else
                {
                    return "RECHAZADO";
                }
            }
            else
            {
                return "RECHAZADO";
            }
            
            //return "ACEPTADO";
        }

        public ActionResult Cierre1()
        {
            string TBK_ORDEN_COMPRA = Request.Form["TBK_ORDEN_COMPRA"];
            string archivo_rechazo = Server.MapPath("~/cgi-bin/log/Rechazo_" + TBK_ORDEN_COMPRA + ".txt");
            if (System.IO.File.Exists(archivo_rechazo))
            {
                System.IO.File.Delete(archivo_rechazo);
            }

            try
            {
                if (check_mac(TBK_ORDEN_COMPRA, Request.Form.ToString()) == true)
                {
                    Response.Write("ACEPTADO");
                    //validación de orden de de compra realizada de manera interna al igual que la validación del monto.
                }
                else
                {
                    StreamWriter writer = new StreamWriter(archivo_rechazo, false, System.Text.Encoding.Default);
                    writer.WriteLine("RECHAZADO, MAC NO VALIDA");
                    writer.Close();
                    Response.Write("RECHAZADO");
                }
            }
            catch (Exception ex)
            {
                StreamWriter writer = new StreamWriter(archivo_rechazo, false, System.Text.Encoding.Default);
                writer.WriteLine("Error :" + ex.ToString());
                writer.Close();
                Response.Write("RECHAZADO");
            }
            string acepta = "ACEPTADO";
            return View(acepta);
        }

        public bool check_mac(string TBK_ORDEN_COMPRA, string str)
        {
            string CarpetaLogs = Server.MapPath("~/cgi-bin/log/");
            string ArchivoBat = Server.MapPath("~/tbk_check_mac.bat");
            string EjecutableCheckMac = Server.MapPath("~/cgi-bin/tbk_check_mac.exe");
            string archivo_temporal = System.IO.Path.Combine(CarpetaLogs, "Input_" + TBK_ORDEN_COMPRA + ".txt");
            string archivo_resultado = System.IO.Path.Combine(CarpetaLogs, "Output_" + TBK_ORDEN_COMPRA + ".txt");
            string cmd = ArchivoBat + " " + EjecutableCheckMac + " " + archivo_temporal + " " + archivo_resultado;
            if (System.IO.File.Exists(archivo_temporal))
            {
                System.IO.File.Delete(archivo_temporal);
            }
            StreamWriter writer = new StreamWriter(Server.MapPath(archivo_temporal), false, System.Text.Encoding.Default);
            writer.WriteLine(str);
            writer.Close();
            System.Diagnostics.Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
            System.IO.StreamReader srFicheroCreado = new System.IO.StreamReader(archivo_resultado);
            string res_check_mac = srFicheroCreado.ReadLine();
            srFicheroCreado.Close();
            return true;
        }

        public ActionResult EvidenciaComercio()
        {
            string document = Server.MapPath("~/Content/EVIDENCIA_DE_FLUJOS.rar");
            string type = "application/zip";
            return File(document, type);
        }

       
    }
}
