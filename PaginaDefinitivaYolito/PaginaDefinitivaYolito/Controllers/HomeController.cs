using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Microsoft.Dynamics.BusinessConnectorNet;
using PaginaDefinitivaYolito.Models;
using System.Net.Mail;
using PagedList;
using System.Globalization;
using MvcBreadCrumbs;
using MvcSiteMapProvider.Web.Mvc.Filters;
using MvcSiteMapProvider;

namespace PaginaDefinitivaYolito.Controllers
{
    public class HomeController : Controller
    {
        
        private PaginaWebEntities1 db = new PaginaWebEntities1();
        private Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
       
        
        public ActionResult Home()
        {
            
            return View();
        }

        public ActionResult ImagenesBanner()
        {
            return PartialView();
        }
        //funcion banner
        public ActionResult OfertasBanner(int id, string Nombre)
        {
            //id el banner seleccionado
            //query . en SQL nativo = select * from ProductosOferta.
            var queryOfertas = from pr in db.ProductosOferta
                               where pr.Banner == id
                               select pr;

            if (queryOfertas.Any() == false)
            {
                ViewBag.error = "Sin coincidencias";
            }
            ViewBag.NombreBanner = Nombre;

            return View(queryOfertas.ToList());
        }


        [HttpPost]
        public ActionResult Suscribete(string newslatter)
        {
            if (!String.IsNullOrEmpty(newslatter))
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
                send_mail.From = new MailAddress("ventasinternet@yolito.cl", "Yolito newslatter");
                send_mail.To.Add(new MailAddress("ventasinternet@yolito.cl"));
                send_mail.Subject = "Cliente inscrito para promociones";
                send_mail.Body = newslatter;


                try
                {
                    smtp.Send(send_mail);
                }
                catch (Exception ep)
                {
                    ViewBag.error = ep.Message;
                }
            }
            

            return View();
        }
        [HttpPost]
        public bool guarda(HttpPostedFileBase file, HttpPostedFileBase fileBN)
        {
            if (file != null && fileBN != null)
            {
                file.SaveAs(Server.MapPath("~/Content/MarcasBD/" + file.FileName));
                fileBN.SaveAs(Server.MapPath("~/Content/MarcasBD/MarcasBN/" + fileBN.FileName));
                return true;
            }

            return false;
        }

        [HttpPost]
        public ActionResult AddImagen(MarcasAsoc model, string nombremarca, HttpPostedFileBase file, HttpPostedFileBase fileBN)
        {
            if (guarda(file, fileBN) == true)
            {
                ViewBag.paso = "Foto guardada en carpeta";

                string pathfoto = Url.Content("~/Content/MarcasBD/" + file.FileName);
                string pathfotoBN = Url.Content("~/Content/MarcasBD/MarcasBN/" + fileBN.FileName);
                if (!String.IsNullOrEmpty(nombremarca))
                {
                    model.NombreMarca = nombremarca;
                }
                
                model.FotoNormal = pathfoto;
                model.FotoBN = pathfotoBN;
                db.MarcasAsoc.Add(model);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception E)
                {
                    ViewBag.error = E.Message;
                }
            }
            else
            {
                ViewBag.error = "No se admite nulo";
            }
            
            return View();
        }

        [HttpPost]
        public ActionResult CheckStock(string itembarcode, string Local, int Cantidad)
        {
            object CheckStock;
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

            CheckStock = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock",
                itembarcode,
                Local,
                Cantidad);
            string Stock = CheckStock.ToString();
            
            DynAx.Logoff();
            return Json(Stock, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult CalculaDespacho(string itembarcode, string Comuna, int Cantidad)
        {
            object CalcFreight;
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

            CalcFreight = DynAx.CallStaticClassMethod("WebPageConection", "CheckStock",
                itembarcode,
                "03",
                Cantidad);

            string Stock = CalcFreight.ToString();

            DynAx.Logoff();
            return Json(Stock, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult AgregarImagenProducto()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AgregarImagenProducto(HttpPostedFileBase file, string codigo)
        {
            if (!String.IsNullOrEmpty(codigo) & codigo == "y0l1t0.2017")
            {
                try
                {
                    if (file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);

                        var path = Path.Combine(Server.MapPath("~/Content/Productos"), fileName);

                        file.SaveAs(path);
                        ViewBag.nombreImg = fileName;
                        ViewBag.Uploaded = "Subida con exito" ;
                    }
                    
                    return View("AgregarImagenProducto");
                }
                catch (Exception op)
                {
                    ViewBag.Uploaded = "Upload failed" + op.Message;
                    return View("AgregarImagenProducto");
                }
            }
            else
            {
                ViewBag.error = "Código erroneo, imagen no agregada";
                return View("AgregarImagenProducto");
            }
            
        }

        [HttpGet]
        public ActionResult ImgMarcas()
        {

            return View(db.MarcasAsoc.ToList());
        }

        
        public ActionResult RenderMarcas()
        {

            return View("SliderMarcas",db.MarcasAsoc.ToList());
        }


        public ActionResult Nosotros()
        {
            

            return View();
        }

        [HttpGet]
        public ActionResult Contacto()
        {
           

            return View();
        }

        [HttpPost]
        public ActionResult Contacto(MailContacto email)
        {
            ViewBag.Message = "Enviar Correo al contacto@yolito.cl";

            return View();
        }

        public ActionResult Politicas()
        {

            return View();
        }
        [HttpGet]
        public ActionResult Trabajo()
        {
            ViewBag.Comunas = db.AddressCounty.OrderBy(c => c.Name).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Trabajo(EnviaCVModel formTrabajo)
        {
            ViewBag.Comunas = db.AddressCounty.OrderBy(c => c.Name).ToList();
            //HttpPostedFileBase file
            DateTime hoydia = DateTime.Today;
            string hoy = hoydia.ToString("dd/MM/yyyy");
            // Verify that the user selected a file
            if (formTrabajo.file != null && formTrabajo.file.ContentLength > 0)
            {
                // extract only the fielname
                var fileName = Path.GetFileName(formTrabajo.file.FileName);
                // store the file inside ~/Content/Curriculums folder
                var path = Path.Combine(Server.MapPath("~/Content/Curriculums"), fileName);
                formTrabajo.file.SaveAs(path);
            }

            try
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
                string textBody = "Se ha recibido una solicitud de trabajo a través del portal web. <br/> <h3>DATOS DEL POSTULANTE </h3> Nombre: <strong>" + formTrabajo.NombreCompleto.ToUpper() + "</strong><br/> Rut: <strong>" + formTrabajo.rut + "</strong><br/> Email: <strong>" + formTrabajo.Email + "</strong><br/> Teléfono Fijo: <strong>" +formTrabajo.TelefonoFijo + "</strong><br/>Telefono Movil: <strong>"+formTrabajo.TelefonoMovil+"</strong><br/> Dirección: <strong>"+formTrabajo.Dirección+"</strong><br/><hr/><h3>Curriculum Adjunto:</h3>"  ;

                if (formTrabajo.file != null && formTrabajo.file.ContentLength > 0)
                {
                    send_mail.Attachments.Add(new Attachment(formTrabajo.file.InputStream, Path.GetFileName(formTrabajo.file.FileName)));
                }
                send_mail.IsBodyHtml = true;
                send_mail.From = new MailAddress("ventasinternet@yolito.cl","Postulacion web Yolito");
                //send_mail.To.Add(new MailAddress("irojas@yolito.cl"));
                send_mail.To.Add(new MailAddress("sspp@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("odiaz@yolito.cl"));
                send_mail.Bcc.Add(new MailAddress("ngaete@yolito.cl"));
                send_mail.Subject = "Solicitud de trabajo a través del portal web." + hoy;
                send_mail.Body = textBody;
                try
                {
                    smtp.Send(send_mail);
                    ViewBag.Message = "Sent";
                }
                catch (Exception ec)
                {
                    Response.Write(ec.Message);
                }
                
                

                return View("Trabajo",formTrabajo);
            }
            catch (Exception ex)
            {
                Response.Write("ERROR: "+ex.Message);
            }
            

            return View("Trabajo", formTrabajo);
        }

       
        //string sortOrder, string CurrentSort, int? page
        
        public ActionResult Productos()
        {
            var n1List = db.Nivel1.ToList();
           
            return View(n1List);
        }


        public ActionResult AutoComplete(string term)
        {
            
            var query = from prod in db.Productos
                        join cat1 in db.Nivel1 on prod.Clasif1 equals cat1.ICat1
                        join cat2 in db.Nivel2 on prod.Clasif2 equals cat2.ICat2
                        join cat3 in db.Nivel3 on prod.Clasif3 equals cat3.ICat3
                        where prod.ItemName.Contains(term) || cat1.Name.Contains(term) || cat2.Name.Contains(term) || cat3.Name.Contains(term) || prod.brandInternet.Contains(term) 
                        select prod.ItemName;

            return Json(query.ToList(), JsonRequestBehavior.AllowGet);

        }
        
        [HttpGet]
        public ActionResult Buscador(string id,string buscar,string FiltroActual,string sortOrder, int? page)
        {
            
            if (!String.IsNullOrEmpty(buscar))
            {
                ViewBag.busqueda = buscar;
                
                ViewBag.CurrentSort = FiltroActual;
                ViewBag.CurrentFilter = buscar;

                var buscaProd = (from prod in db.Productos
                                join cat1 in db.Nivel1 on prod.Clasif1 equals cat1.ICat1
                                join cat2 in db.Nivel2 on prod.Clasif2 equals cat2.ICat2
                                join cat3 in db.Nivel3 on prod.Clasif3 equals cat3.ICat3
                                where prod.ItemName.Contains(buscar) || cat1.Name.Contains(buscar) || cat2.Name.Contains(buscar) || cat3.Name.Contains(buscar) || prod.brandInternet.Contains(buscar)
                                 select prod).OrderBy(c => c.Orden).ThenByDescending(c => c.HighLights).ThenBy(c => c.Clasif3).ThenBy(c => c.ItemName);

                var buscaprodCat1 = (from pr in db.Nivel1
                                     where pr.Name.Contains(buscar)
                                     select pr).OrderBy(c => c.Name);
                                     

                if (buscaProd.Count() == 0)
                {
                    ViewBag.error = ("No tenemos ese producto actualmente");
                }

                switch (FiltroActual)
                {
                    case "Default":
                        buscaProd = buscaProd.OrderBy(c => c.ItemName);
                        break;

                    case "Menor":
                        buscaProd = buscaProd.OrderBy(c => c.Price);
                        break;

                    case "Mayor":
                        buscaProd = buscaProd.OrderByDescending(c => c.Price);
                        break;

                    case "Marca":
                        buscaProd = buscaProd.OrderBy(c => c.brandInternet);
                        break;
                }
                
                int pageSize = 8;

                return View(buscaProd.ToList().ToPagedList(page ?? 1, pageSize));
            }
            else
            {
                
                return PartialView("Nobusca");

            }
            
        }

        
        public ActionResult ProductosMarca(string nombremarca, string FiltroActual, int? page)
        {
            
            ViewBag.name = nombremarca;

            ViewBag.CurrentSort = FiltroActual;

            var Encontre = (from p in db.Productos
                            where p.brandInternet.Contains(nombremarca)
                            select p);
           

            if (Encontre.Count() == 0)
            {
                ViewBag.erro = "No hay productos para esta marca";
            }
            int pageSize = 8;

            switch (FiltroActual)
            {
                case "Default":
                    Encontre = Encontre.OrderBy(c => c.ItemName);
                    break;

                case "Menor":
                    Encontre = Encontre.OrderBy(c => c.Price);
                    break;

                case "Mayor":
                    Encontre = Encontre.OrderByDescending(c => c.Price);
                    break;
                
            }

            return View(Encontre.OrderBy(c => c.HighLights).ThenBy(c => c.Orden).ThenBy(q => q.Clasif3).ThenByDescending(o => o.ItemName).ToList().ToPagedList(page ?? 1, pageSize));
        }

        public ActionResult SomeAction()
        {
            
            return View();
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
            send_mail.To.Add(new MailAddress("irojas@yolito.cl"));
            //send_mail.To.Add(new MailAddress("lpizarro@yolito.cl"));
            send_mail.BodyEncoding = System.Text.Encoding.UTF8;
            //send_mail.Bcc.Add(new MailAddress("ccerda@yolito.cl"));

            string textBody = "<h1>Comprobante de venta internet AWeb_" + id + "</h1></br><h4>Datos del cliente:</h4></br><table><tr><td>Rut del cliente:</td><td>" + getCarroPagado.RutCliente + "</td></tr><tr><td>Nombre del cliente:</td><td>" + getCarroPagado.NombreCliente + "</td></tr><tr><td>Código compra: </td><td>" + getCarroPagado.CodigoCompraTBK + "</td></tr><tr><td>Número de tarjeta:</td><td>" + getCarroPagado.NumTarjetaTBK + "</td></tr><tr><td>Numero de cuotas:</td><td>" + getCarroPagado.NCuotasTBK + "</td></tr><tr><td>Fecha de la compra:</td><td>" + getCarroPagado.FechaCompra + "</td></tr><tr><td>Monto:</td><td>" + getCarroPagado.Monto + "</td></tr><tr><td>Tipo de cuota:</td><td>" + getCarroPagado.TipoCuota + "</td></tr><tr><td>Email del cliente:</td><td>" + getCarroPagado.EmailCliente + "</td></tr></table>";

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
        
      
        public ActionResult Categoria1(string id, string nombre, string FiltroActual, int? page )
        {

            Session["cat1"] = nombre;
            Session["Idcat1"] = id;
            
            int pageSize = 8;
            ViewBag.nivel2 = db.Nivel2.Where(p => p.ICat1 == id).ToList();
            ViewBag.CurrentSort = FiltroActual;
            ViewBag.name = nombre;
            ViewBag.Categoria1 = id;

            var Encontre = (from p in db.Productos
                           where p.Clasif1 == id
                            select p).OrderBy(c => c.HighLights).ThenBy(c=>c.Orden).ThenBy(q => q.Clasif3).ThenByDescending(o => o.ItemName);

            if (Encontre.Count() == 0)
            {
                ViewBag.error = ("No hay productos para esta categoria");
            }
            
           
            switch (FiltroActual)
            {
                case "Default":
                    Encontre = Encontre.OrderBy(c => c.ItemName);
                    break;

                case "Menor":
                    Encontre = Encontre.OrderBy(c => c.Price);
                   break;

                case "Mayor":
                   Encontre = Encontre.OrderByDescending(c => c.Price);
                   break;

                case "Marca":
                   Encontre = Encontre.OrderBy(c => c.brandInternet);
                    break;
            }
            long recrefid = Convert.ToInt64(Encontre.FirstOrDefault().RefRecId);
            if (ProductosKit(recrefid) == true)
            {
                ViewBag.Kit = db.ProductosKit.ToList();
            }
            else
            {
                ViewBag.NOKit = "Sin kit";
            }
           
            return Request.IsAjaxRequest()
                ? (ActionResult)PartialView("ProductList",Encontre.ToList().ToPagedList(page ?? 1, pageSize))
                : View(Encontre.ToList().ToPagedList(page ?? 1, pageSize));
        }

        public ActionResult Categoria2(string id,string N2, string Name1,string Name2, string FiltroActual, int? page)
        {
            ViewBag.NameCat1 = Name1;
            ViewBag.IdCat1 = id;
           
            var queryGetNivel1 = (from n1 in db.Nivel1
                                  join n2 in db.Nivel2 on n1.ICat1 equals n2.ICat1
                                  where n1.ICat1 == id
                                  select n1);
            
            
            
            ViewBag.Cat2 = N2;
            ViewBag.name = Name2;
            
            
            int pageSize = 8;
            ViewBag.nivel3 = db.Nivel3.Where(c => c.ICat2 == N2).ToList();
            ViewBag.CurrentSort = FiltroActual;

            var Encontre = db.Productos.Where(p => p.Clasif2 == N2).OrderBy(c => c.Orden).ThenBy(c=>c.HighLights).ThenBy(p => p.Clasif3).ThenBy(u => u.ItemName);

            if (Encontre.Count() == 0)
            {
                ViewBag.error = ("No hay productos para esta categoria");
            }

            switch (FiltroActual)
            {
                case "Default":
                    Encontre = Encontre.OrderBy(c => c.ItemName);
                    break;

                case "Menor":
                    Encontre = Encontre.OrderBy(c => c.Price);
                    break;

                case "Mayor":
                    Encontre = Encontre.OrderByDescending(c => c.Price);
                    break;

                case "Marca":
                    Encontre.OrderBy(c => c.brandInternet);
                    break;
            }

            long recrefid = Convert.ToInt64(Encontre.FirstOrDefault().RefRecId);
            if (ProductosKit(recrefid) == true)
            {
                ViewBag.Kit = db.ProductosKit.ToList();
            }
            else
            {
                ViewBag.NOKit = "Sin kit";
            }

            return View(Encontre.ToList().ToPagedList(page ?? 1, pageSize));
        }


        public ActionResult Categoria3(string id, string N2, string N3, string Name1, string Name2, string Name3, string FiltroActual, int? page)
        { //id es cat2 
            ViewBag.NameCat1 = Name1;
            ViewBag.IdCat1 = id;

            ViewBag.Nivel2 = db.Nivel2.Where(c => c.ICat2 == N2).ToList();


            ViewBag.Cat2 = N2;
            ViewBag.NameCat2 = Name2;

            ViewBag.CurrentSort = FiltroActual;

            ViewBag.nombre = Name3;
            ViewBag.Categoria3 = N3;
            
            var Encontre = db.Productos.Where(p => p.Clasif3 == N3).OrderBy(c => c.Orden).ThenBy(c=>c.HighLights).ThenBy(p => p.ItemName);

            if (Encontre.Count() == 0)
            {
                ViewBag.error = ("No hay productos para esta categoria");
            }
            
            int pageSize = 8;

            switch (FiltroActual)
            {
                case "Default":
                    Encontre = Encontre.OrderBy(c => c.ItemName);
                    break;

                case "Menor":
                    Encontre = Encontre.OrderBy(c => c.Price);
                    break;

                case "Mayor":
                    Encontre = Encontre.OrderByDescending(c => c.Price);
                    break;

                case "Marca":
                    Encontre.OrderBy(c => c.brandInternet);
                    break;
            }
            
            return Request.IsAjaxRequest()
                ? (ActionResult)PartialView("ProductList", Encontre.ToList().ToPagedList(page ?? 1, pageSize))
                : View(Encontre.ToList().ToPagedList(page ?? 1, pageSize));
        }

        public ActionResult GetCount()
        {
            if (Session["cart"] != null)
            {
                var nped = Session["cart"].ToString();
                int elped = Convert.ToInt32(nped);
                var count = (from cr in db.DetalleCarro
                             where cr.NPedido == elped
                             select cr.Cantidad).Sum();

                return PartialView(count);
            }
            else
            {
                return PartialView();
            }
        }

        public ActionResult ConvertidorMT2(string id, int cantidadCajas ,long ftec)
        {
            //id es itembarcode
            var getProducto = (from pr in db.Productos
                               where pr.ItemBarCode == id && pr.RefRecId == ftec
                               select pr).FirstOrDefault();
            var getValorCaja = getProducto.Factor * getProducto.Price;
            var productos = (from p in db.Productos
                             where p.ItemBarCode == id
                             orderby p.ItemName
                             select p);
            ViewBag.breadcrum = productos.ToList();
            var valorXcaja = getValorCaja * cantidadCajas;
            //luego de calcular la cantidad de cajas en Valorxcaja calcular la cantidad de metros cuadrados
            double? mt2Xcaja = cantidadCajas * getProducto.Factor;
            decimal mt2Total = Math.Round(Convert.ToDecimal(mt2Xcaja), 2);

            ViewBag.valorCaja = valorXcaja ;
            ViewBag.mt2Xcaja = mt2Xcaja;
            
            return Json(mt2Total, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult FichaTecnica(string id,string nombre, long ftec)
        {
            
            ViewBag.nombre = nombre;
            ViewBag.comunas = db.AddressCounty.Where(c => c.DlvRoute != "").OrderBy(c => c.Name).ToList();

            var productos = (from p in db.Productos
                            where p.ItemBarCode == id 
                            orderby p.ItemName
                            select p);

            var prodFirst = productos.FirstOrDefault();

            if (prodFirst.Factor != 0)
            {
                var getValorCaja = (prodFirst.Factor * prodFirst.Price);
                ViewBag.valorCaja = getValorCaja;
            }

            var nivel1 = (from n1 in db.Nivel1
                          where n1.ICat1 == prodFirst.Clasif1
                          select n1);

            ViewBag.IdCat1 = nivel1.FirstOrDefault().ICat1;
            ViewBag.nivel1 = nivel1.ToList();
            ViewBag.NameCat1 = nivel1.FirstOrDefault().Name;

            var nivel2 = (from n2 in db.Nivel2
                          where n2.ICat2 == prodFirst.Clasif2
                          select n2);

            ViewBag.NameCat2 = nivel2.FirstOrDefault().Name;
            ViewBag.IdCat2 = nivel2.FirstOrDefault().ICat2;
            ViewBag.Nivel2 = nivel2.ToList() ;

            var nivel3 = (from n3 in db.Nivel3
                          where n3.ICat3 == prodFirst.Clasif3
                          select n3);
            ViewBag.NameCat3 = nivel3.FirstOrDefault().Name;
            ViewBag.Nivel3 = nivel3.ToList();


            if (productos.Any() == false)
            {
                
                ViewBag.err = ("No se encuentra producto");
            }

            if (ProductosKit(ftec) == true)
            {
                ViewBag.Kit = db.ProductosKit.ToList();
            }
            else
            {
                ViewBag.NOKit = "Sin kit";
            }
            return View(productos.ToList());
        }

        public ActionResult DetalleFTecnica(long id)
        {
            var queryFtec = (from ft in db.FichaTecnica
                            where ft.RefRecId == id
                            select ft).OrderBy(p => p.ItemNameAttibute);

            return PartialView(queryFtec.ToList());
        }

        public ActionResult ProductosRelacionados(long id, string itemId, string ConfigId, string InventSizeId)
        {
            var queryProductoRelacionado = (from pdrel in db.ProductosRelacionados
                                            join prod in db.Productos on pdrel.RefRecId equals prod.RefRecId
                                            where pdrel.RefRecId == id
                                            select pdrel).ToList();//productos encontrados..

            //limpiar tabla
            var removetemp = from cc in db.temporal
                             select cc;
            foreach (var rr in removetemp.ToList())
            {
                db.temporal.Remove(rr);
            }
            db.SaveChanges();

            foreach (var item in queryProductoRelacionado)
            {
                var query = (from pr in db.Productos
                            where pr.ItemId == item.ItemId && pr.ConfigId == item.ConfigId && pr.InventSizeId == item.InventSizeId
                            && pr.InventColorId == item.InventColorId
                            select pr);

                var qr = query.FirstOrDefault();
                if (query.Any() == true)
                {
                    //insert en temporal
                    temporal temp = new temporal { 
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

                    db.temporal.Add(temp);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ViewBag.error = (e.Message );
                    }
                    
                }
                
            }


            return PartialView(db.temporal.ToList());
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
                                     where pr.ItemId == item.ItemId && pr.ConfigId == item.ConfigId && pr.InventSizeId ==       item.InventSizeId 
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

        public ActionResult LosMasVendidos()
        {

            return PartialView();
        }

        
        
        public ActionResult PoliticasDespacho()
        {

            return View();
        }

        public bool CargaProductoRelacionado()
        {
            AxaptaRecord axRecord;

            var queryProd = (from p in db.ProductosRelacionados
                             select p).ToList();
            try
            {
                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                using (axRecord = DynAx.CreateAxaptaRecord("InventConbinationItemRelation"))
                {
                    axRecord.ExecuteStmt("select * from %1");
                    if (queryProd.Count == 0)
                    {
                        int count = 0;
                        while (axRecord.Found)
                        {

                            ProductosRelacionados ProdRel = new ProductosRelacionados
                            {
                                IdProd = count++,
                                ItemId = Convert.ToString(axRecord.get_Field("ItemId")),
                                ConfigId = Convert.ToString(axRecord.get_Field("ConfigId")),
                                InventSizeId = Convert.ToString(axRecord.get_Field("InventSizeId")),
                                InventColorId = Convert.ToString(axRecord.get_Field("InventColorId")),
                                RefRecId = Convert.ToInt64(axRecord.get_Field("RefRecId")),
                                
                            };
                            db.ProductosRelacionados.Add(ProdRel);

                            axRecord.Next();
                        }
                        db.SaveChanges();
                        
                        DynAx.Logoff();
                        return true;
                    }

                }

            }
            catch (Exception pp)
            {
                ViewBag.error = pp.Message;
                return false;
            }
            return false;
            
        }

        public void deleteProductos()
        {
            var queryProd = (from p in db.Productos
                             select p);
            foreach (var it in queryProd)
            {
                db.Productos.Remove(it);
            }
            db.SaveChanges();
        }
        public void deleteProductoRelacionado()
        {
            var queryProd = (from p in db.ProductosRelacionados
                             select p).ToList();

            foreach (var it in queryProd)
            {
                db.ProductosRelacionados.Remove(it);
            }
            db.SaveChanges();
        }
        public ActionResult CargaProductos()
        {

            AxaptaRecord axRecord;

            var queryProd = (from p in db.Productos
                                select p);

            int precio = 0;
            
            try
            {
                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
                //eliminar productos de mi tabla
                deleteProductos();
                //cargar ficha Tecnica
                DeleteFichatecnica();
                //cargar la ficha Tecnica
                cargaFichaTecnica();
                //carga productos relacionados
                deleteProductoRelacionado();

                CargaProductoRelacionado();


                using (axRecord = DynAx.CreateAxaptaRecord("TmpInternetItem"))
                {
                    axRecord.ExecuteStmt("select * from %1");
                    if (queryProd.Count() == 0)
                    {
                        int count = 0;
                        while (axRecord.Found)
                        {
                            precio = Convert.ToInt32(axRecord.get_Field("Price"));
                            
                            Productos prodd = new Productos
                            {
                                IdProd = count++,
                                ItemId = Convert.ToString(axRecord.get_Field("ItemId")),
                                ItemName = Convert.ToString(axRecord.get_Field("ItemName")),
                                Clasif1 = Convert.ToString(axRecord.get_Field("Clasif1")),
                                Clasif2 = Convert.ToString(axRecord.get_Field("Clasif2")),
                                Clasif3 = Convert.ToString(axRecord.get_Field("Clasif3")),
                                ConfigId = Convert.ToString(axRecord.get_Field("configId")),
                                InventSizeId = Convert.ToString(axRecord.get_Field("InventSizeId")),
                                InventColorId = Convert.ToString(axRecord.get_Field("InventColorId")),
                                ItemBarCode = Convert.ToString(axRecord.get_Field("itemBarCode")),
                                Price = precio,
                                Photo = Convert.ToString(axRecord.get_Field("Photo")),
                                Stock = Convert.ToString(axRecord.get_Field("Stock")),
                                HighLights = Convert.ToString(axRecord.get_Field("Highlights")),
                                ICat3 = Convert.ToString(axRecord.get_Field("ICat3")),
                                brandInternet = Convert.ToString(axRecord.get_Field("brandInternet")),
                                RefRecId = Convert.ToInt64(axRecord.get_Field("RefRecId")),
                                UnitWeb = Convert.ToString(axRecord.get_Field("UnitWeb")),
                                Factor = Convert.ToDouble(axRecord.get_Field("Factor")),
                                Orden = Convert.ToInt32(axRecord.get_Field("Orden")),
                               // Banner = Convert.ToInt32(axRecord.get_Field("Banner"))
                            };
                            db.Productos.Add(prodd);

                            axRecord.Next();
                        }
                        try
                        {
                            db.SaveChanges();
                            ViewBag.add = "Listo";
                            return View();
                        }
                        catch (Exception r)
                        {
                            ViewBag.error = (r.Message);
                        }

                        DynAx.Logoff();
                    }
                    else
                    {
                        //actualizar precio
                        var upProd = queryProd.FirstOrDefault();
                        while (axRecord.Found)
                        {
                            precio = Convert.ToInt32(axRecord.get_Field("Price"));
                            //upProd.ItemName = Convert.ToString(axRecord.get_Field("ItemName"));
                            upProd.Price = precio;
                            upProd.Photo = Convert.ToString(axRecord.get_Field("Photo"));
                            upProd.HighLights = Convert.ToString(axRecord.get_Field("Highlights"));
                            axRecord.Next();
                        }
                        try
                        {
                            db.SaveChanges();
                           
                            ViewBag.add = "Actualizada";
                            return View();
                        }
                        catch (Exception r)
                        {
                            ViewBag.error = (r.Message);
                        }

                        DynAx.Logoff();

                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.error=(e.Message);
            }  
            return View("Home");
        }

        public void DeleteFichatecnica()
        {
            var queryProd = (from p in db.FichaTecnica
                             select p).ToList();
            foreach (var i in queryProd)
            {
                db.FichaTecnica.Remove(i);
            }
            db.SaveChanges();
        }
        public bool cargaFichaTecnica()
        {
            AxaptaRecord axRecord;

            var queryProd = (from p in db.FichaTecnica
                                select p).ToList();

            try
            {
                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                using (axRecord = DynAx.CreateAxaptaRecord("InventItemDataSheet"))
                {
                    axRecord.ExecuteStmt("select * from %1");
                    string myvar = null;
                    if (queryProd.Count == 0)
                    {
                        while (axRecord.Found)
                        {
                            myvar = Convert.ToString(axRecord.get_Field("ItemAttibute"));
                            FichaTecnica ftecnica = new FichaTecnica
                            {
                                RefRecId = Convert.ToInt64(axRecord.get_Field("RefRecId")),
                                ItemNameAttibute = Convert.ToString(axRecord.get_Field("ItemNameAttibute")),
                                ItemAttibute = myvar
                            };
                            axRecord.Next();
                            db.FichaTecnica.Add(ftecnica);
                           
                        }

                        try
                        {
                            db.SaveChanges();
                            ViewBag.add = "Listo";
                            return true;
                        }
                        catch (Exception ppp)
                        {
                            Response.Write(ppp.Message);
                            
                        }
                        DynAx.Logoff();
                        return false;
                    }
                    else
                    {
                        //tiene datos la tabla
                        string myvar1 = null;
                        foreach (var item in queryProd)
                        {
                            while (axRecord.Found)
                            {
                                myvar1 = Convert.ToString(axRecord.get_Field("ItemAttibute"));
                                item.RefRecId = Convert.ToInt64(axRecord.get_Field("RefRecId"));
                                item.ItemAttibute = myvar1;
                                item.ItemNameAttibute =Convert.ToString(axRecord.get_Field("ItemNameAttibute")) ;

                                axRecord.Next();
                                
                            }
                            
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ppp)
                        {
                            ViewBag.error = (ppp.Message);
                        }
                        DynAx.Logoff();

                    }

                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }

            return false;
        }

        public bool InsertaNivel1()
        {
            AxaptaRecord axRecord;
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            using (axRecord = DynAx.CreateAxaptaRecord("InterCat1"))
            {

                    axRecord.ExecuteStmt("select * from %1");
                    while (axRecord.Found)
                    {
                        Nivel1 nivel1 = new Nivel1
                        {
                            ICat1 = Convert.ToString(axRecord.get_Field("ICat1")),
                            Name = Convert.ToString(axRecord.get_Field("Name")),
                            NameAlias = Convert.ToString(axRecord.get_Field("NameAlias"))
                        };
                        axRecord.Next();
                        db.Nivel1.Add(nivel1);
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ok)
                    {
                        ViewBag.error = (ok.Message);
                    }
            }
            DynAx.Logoff();
            return true;
        }

        public bool InsertaNivel2()
        {
            AxaptaRecord axRecord;
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            using (axRecord = DynAx.CreateAxaptaRecord("InterCat2"))
                {

                    axRecord.ExecuteStmt("select * from %1");
                    while (axRecord.Found)
                    {
                        Nivel2 nivel2 = new Nivel2
                        {
                            ICat1 = Convert.ToString(axRecord.get_Field("ICat1")),
                            ICat2 = Convert.ToString(axRecord.get_Field("ICat2")),
                            Name = Convert.ToString(axRecord.get_Field("Name")),
                            NameAlias = Convert.ToString(axRecord.get_Field("NameAlias"))
                        };
                        axRecord.Next();
                        db.Nivel2.Add(nivel2);
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception okkk)
                    {
                        ViewBag.error = (okkk.Message);
                    }
            }
            DynAx.Logoff();
            return true;
        }

        public bool InsertaNivel3()
        {
            AxaptaRecord axRecord;
            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            using (axRecord = DynAx.CreateAxaptaRecord("InterCat3"))
            {
                axRecord.ExecuteStmt("select * from %1");
                while (axRecord.Found)
                {
                    Nivel3 nivel3 = new Nivel3
                    {
                        ICat2 = Convert.ToString(axRecord.get_Field("ICat2")),
                        ICat3 = Convert.ToString(axRecord.get_Field("ICat3")),
                        Name = Convert.ToString(axRecord.get_Field("Name")),
                        NameAlias = Convert.ToString(axRecord.get_Field("NameAlias")),
                        Link = Convert.ToString(axRecord.get_Field("NameAlias")),
                        Orden = Convert.ToInt32(axRecord.get_Field("Orden"))
                    };
                    axRecord.Next();
                    db.Nivel3.Add(nivel3);
                }
                
                
                try
                {
                    db.SaveChanges();
                }
                catch (Exception okkk)
                {
                    ViewBag.error = (okkk.Message);
                }
            }
            DynAx.Logoff();
            return true;
        }

        public bool deleteNiveles()
        {
            var queryProd = (from p in db.Nivel1
                             select p);
            foreach (var it in queryProd)
            {
                db.Nivel1.Remove(it);
            }
            
            var queryProd2 = (from p in db.Nivel2
                             select p);
            foreach (var it in queryProd2)
            {
                db.Nivel2.Remove(it);
            }

            var queryProd3 = (from p in db.Nivel3
                             select p);
            foreach (var it in queryProd3)
            {
                db.Nivel3.Remove(it);
            }
            try
            {
                db.SaveChanges();
                return true;
            }
            catch(Exception op)
            {
                TempData["EN"] = op.Message;
                return false;
            }
            
            
        }
        public ActionResult CargaNiveles()
        {

            if (deleteNiveles() == true)
            {
                if (InsertaNivel1() == true)
                {
                    if (InsertaNivel2() == true)
                    {
                        if (InsertaNivel3() == true)
                        {
                            ViewBag.add = "Actualizadas las categorias";
                            return View();
                        }
                    }
                }
            }
            
            
            return View("Home");
        }



        
    }
}
