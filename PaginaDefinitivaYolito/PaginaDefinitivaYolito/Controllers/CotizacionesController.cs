using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;
using System.Net.Mail;
using System.Net;

namespace PaginaDefinitivaYolito.Controllers
{
    public class CotizacionesController : Controller
    {
        //
        // GET: /Cotizaciones/

        private PaginaWebEntities1 db = new PaginaWebEntities1();

        public ActionResult Index()
        {
            return View();
        }

        
        [HttpGet]
        public ActionResult QuieroCotizar()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult EnviarCotizacion(EmailCotizacion Cotizacion)
        {
            if (ModelState.IsValid)
            {
                
                DateTime hoydia = DateTime.Today;
                string fecha_exito = hoydia.ToString("dd/MM/yyyy");

                Cotizaciones ICotiza = new Cotizaciones { 
                    NombreCliente = Cotizacion.nombre_cotizante,
                    Apellido = Cotizacion.apellido_cotizante,
                    Email = Cotizacion.From,
                    TelefonoContacto = Cotizacion.telefono_cotizante,
                    Asunto = Cotizacion.Subject,
                    BodyEmail = Cotizacion.Body,
                    FechaCotiza = hoydia,
                };
                

                db.Cotizaciones.Add(ICotiza);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception p)
                {
                    ViewBag.error = p.Message;
                }

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
                send_mail.From = new MailAddress(Cotizacion.From);
                send_mail.To.Add(new MailAddress("ventasinternet@yolito.cl", "Cotizacion Yolito"));
                send_mail.Bcc.Add(new MailAddress("ayoliton@yolito.cl"));

                send_mail.Subject = Cotizacion.Subject + "Fecha: " + fecha_exito;
                send_mail.Body = "Nombre del cliente: " + Cotizacion.nombre_cotizante + "<br />" + "Email del cliente: "
                    + Cotizacion.From + "<br />" + "Teléfono del cliente: " + Cotizacion.telefono_cotizante + "<br />"
                    + "Fecha de envio: " + fecha_exito + "<br />" + "Detalle de la cotización: " + "<br />" + Cotizacion.Body;

                smtp.Send(send_mail);
            }
            else
            {
                ViewBag.error = ("formulario no valido");
            }

            return View();
        }

        public ActionResult CarroComoCotizacion(int? id)
        {
            var encontre = (from pr in db.DetalleCarro
                            where pr.NPedido == id
                            select pr);

            return View(encontre.ToList());
        }

        public void sendCotizacion()
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
            //send_mail.IsBodyHtml = true;
            //send_mail.From = new MailAddress(Cotizacion.From);
            //send_mail.To.Add(new MailAddress("ventasinternet@yolito.cl"));
            //send_mail.Subject = Cotizacion.Subject + "Fecha: " + fecha_exito;
            //send_mail.Body = "Nombre del cliente: " + Cotizacion.nombre_cotizante + "<br />" + "Email del cliente: "
            //    + Cotizacion.From + "<br />" + "Teléfono del cliente: " + Cotizacion.telefono_cotizante + "<br />"
            //    + "Fecha de envio: " + fecha_exito + "<br />" + "Detalle de la cotización: " + "<br />" + Cotizacion.Body;

            smtp.Send(send_mail);
        }


    }
}
