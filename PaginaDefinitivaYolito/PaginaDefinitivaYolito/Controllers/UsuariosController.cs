using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.Web.Security;
using System.Net.Mail;
using WebMatrix.WebData;

namespace PaginaDefinitivaYolito.Controllers
{
    public class UsuariosController : Controller
    {
        //
        // GET: /Usuarios/
        private PaginaWebEntities1 db = new PaginaWebEntities1();
        private Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
        private System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public JsonResult validaUsuario(string rut)
        {
            object getUsuario;
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            getUsuario = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", rut);
            string responseUser = getUsuario.ToString();
            string[] UsSplit = responseUser.Split('|');
            string existeEnAX = UsSplit[0];
            string rutUsuario = UsSplit[1];
            string pw = UsSplit[2];
            string EmailCliente = UsSplit[4];

            if (existeEnAX == "TRUE")
            {
                if (String.IsNullOrEmpty(pw))
                {
                    ViewBag.SinPw = "No cuentas con password registrada";
                }
            }
            else
            {
                ViewBag.NoExisteCliente = "No existes como cliente en el sistema";
            }


            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LogIn(UserModel users, string ReturnUrl)
        {
            
            var usmodel = new UserModel();
            //validar si el usuario ya tiene una sesion activa con un npedido asociado
            if (ModelState.IsValid)
            {

                
                //var user = db.Clientes.FirstOrDefault(u => u.RutCliente == users.rut);
                //var pass = db.Clientes.FirstOrDefault(u => u.Password == users.password);
                //var comp = db.CabeceraCarro.Any(c => c.RutCliente == users.rut);
                //var existe_usuario = (from c in db.Clientes
                //                         where c.RutCliente + "-" + c.DigCliente == users.rut
                //                     select c);
                object getUsuario;

                try
                {
                    Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                    System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                    DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
                    try
                    {

                        string usuario = users.rut;
                        getUsuario = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", usuario);
                        string resp = getUsuario.ToString();

                        string[] usuario1 = resp.Split('|');
                        string existeEnAX = usuario1[0];

                        
                        if (existeEnAX == "TRUE")
                        {
                            string rutUsuario = usuario1[1];
                            string pw = usuario1[2];

                            string[] splitRutUser = rutUsuario.Split('-');
                            string theRut = splitRutUser[0];
                            int numRut = theRut.Length;
                            //con la pw nueva. guardo en AX el email y la contraseña nueva.
                            string pwNueva = theRut.Substring(numRut - 4, 4);

                            string NombreCliente = usuario1[3];
                            
                            string EmailCliente = usuario1[4];
                            if (users.password == pwNueva)
                            {
                                return RedirectToAction("Recuperar", "Usuarios", new { id = rutUsuario});
                            }
                            else
                            {
                                if (pw == users.password)
                                {
                                    //si loguea
                                    Session["nombreuser"] = NombreCliente;
                                    Session["rutuser"] = rutUsuario;
                                    Session["emailuser"] = EmailCliente;
                                    TempData["logueado"] = "Sent";

                                }
                                else
                                {
                                    //no loguea
                                    ViewBag.badpw = "Contraseña Incorrecta";
                                    //Response.Write("Contraseña Incorrecta");
                                    usmodel.rut = rutUsuario;
                                    usmodel.password = pw;

                                }
                            }
                            
                        }

                    }
                    catch (Exception ex)
                    {
                        ViewBag.error = (ex.Message);
                    }
                    

                }
                catch (Exception ex)
                {
                    ViewBag.error = ("NO HUBO CONEXION CON EL SISTEMA, ERROR: " + ex.Message);
                }
               
                
            }

            return View(users);
        }

        [NoCache]
        public ActionResult LogOut()
        {
            Session.Clear();

            return RedirectToAction("Home", "Home");
        }

        [HttpGet]
        public ActionResult ReinicioContrasena()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ReinicioContrasena(ResetPassword formReset, string email)
        {
            if (ModelState.IsValid)
            {
                string rutUsuario = formReset.rut;
                object o;
                try
                {
                    Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                    System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                    DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
                    o = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", rutUsuario);
                    string resp = o.ToString();

                    string[] usuario1 = resp.Split('|');
                    string existe = usuario1[0];
                    string rutUser = usuario1[1];
                    if (existe == "TRUE")
                    {
                        //si el usuario existe, rescatar los Ultimos 4 números del rut
                        string[] splitRutUser = rutUser.Split('-');
                        string theRut = splitRutUser[0];
                        int numRut = theRut.Length;
                        //con la pw nueva. guardo en AX el email y la contraseña nueva.
                        string pwNueva = theRut.Substring(numRut - 4, 4);

                        //ingresar datos al AX, CreateCustTable
                        object UpdateCustTable;
                        UpdateCustTable = DynAx.CallStaticClassMethod("WebPageConection", "UpdateCustPassword", rutUser, pwNueva);
                        if (UpdateCustTable.ToString() == "True")
                        {
                            ViewBag.OkReset = "Contraseña actualizada";
                            //enviar mail con link de recuperación
                            string url = Url.Action("Recuperar", "Usuarios",
    new System.Web.Routing.RouteValueDictionary(new { id = formReset.rut }),
    "http", Request.Url.Host);
                            SendEMail(email, url);

                        }
                        else
                        {
                            ViewBag.errorUpdatePw = "No se pudo recuperar la contraseña";
                        }


                    }
                    else
                    {
                        ViewBag.message2 = "No existes como usuario";
                    }
                    DynAx.Logoff();
                }
                catch (Exception ex)
                {
                    ViewBag.error = ("ERROR: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Formulario no válido");
            }
            return View();
        }
        private void SendEMail(string emailCliente, string Url)
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;


            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("ventasinternet@yolito.cl", "ventasweb.2017");
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.From = new MailAddress("ventasinternet@yolito.cl", "Recuperación clave Yolito");
            msg.To.Add(new MailAddress(emailCliente));

            msg.Subject = "Recuperar clave yolito";
            msg.IsBodyHtml = true;

            string body = "<h3>Estimado cliente: </h3>";
            string body1 = body + "<p>su contraseña fue recuperada con exito.</p><b>su contraseña son los últimos 4 digitos del rut sin guión</b><p>Para reiniciar su contraseña, favor entrar en el siguiente <h2><a href= " + Url + ">Link</a></h2>  para finalizar el proceso.</p><h3>Gracias por preferirnos.</h3>";
            msg.Body = body1;

            client.Send(msg);
        }

        [HttpGet]
        public ActionResult Recuperar(string id)
        {
            //id es el rut del usuario
            
            object getUser;

            Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
            System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
            DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");
            getUser = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", id);

            string resp = getUser.ToString();
            string[] usuario1 = resp.Split('|');
            string existe = usuario1[0];
           

            if (existe == "TRUE")
            {
                ViewBag.rutUsuario = id;
                
            }
            else
            {
                ViewBag.noexistes = "No existes como cliente";
            }
            DynAx.Logoff();
            return View();
        }

        [HttpPost]
        public ActionResult Recuperar(Resetpw formPw, string id, string OldPw)
        {
            if (ModelState.IsValid)
            {
                object o;
                object getUser;

                Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                
                getUser = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", id);
                string resp = getUser.ToString();
                string[] usuario1 = resp.Split('|');
                string pw = usuario1[2];

                if (OldPw == pw)
                {
                    if (OldPw != formPw.ConfirmPassword)
                    {
                        o = DynAx.CallStaticClassMethod("WebPageConection", "UpdateCustPassword", id, formPw.ConfirmPassword);
                        if (o.ToString() == "True")
                        {
                            //var updatepw = from cli in db.Clientes
                            //               where id = cli.RutCliente
                            //               select cli;
                            ViewBag.cambio = "CONTRASEÑA CAMBIADA CON EXITO";
                        }
                    }
                    else
                    {
                        ViewBag.badOldpw = "La contraseña nueva no puede ser igual ";
                    }
                    
                }
                else
                {
                    ViewBag.badOldpw = "No coincide la contraseña antigua";
                }
                DynAx.Logoff();
            }
            else
            {
                ModelState.AddModelError("", "Formulario no válido");
            }


            return View();
        }

        [HttpGet]
        public ActionResult Registration()
        {
            ViewBag.Comunas = db.AddressCounty.Where(c => c.StateId == "Región Met").OrderBy(c => c.Name).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Registration(DatosPersonalesModel user)
        {
            
            ViewBag.Comunas = db.AddressCounty.Where(c => c.StateId == "Región Met").OrderBy(c => c.Name).ToList();
            if (ModelState.IsValid)
            {
                string rut = user.rut;
                string[] rutus = Convert.ToString(rut).Split('-');
                string rut1 = rutus[0];
                string dig1 = rutus[1];

                //var existeuser = (from u in db.Clientes
                //                  where u.RutCliente + "" + u.DigCliente == rut1 + "" + dig1
                //                  select u).FirstOrDefault();

                //if (existeuser == null)
                //{
                //    var crearuser = db.Clientes.Create();
                //    crearuser.RutCliente = rut1;
                //    crearuser.DigCliente = dig1;
                //    crearuser.Nombre = user.Nombre;
                //    crearuser.Apellido = user.Apellido;
                //    crearuser.Email = user.Email;
                //    crearuser.Direccion = user.Direccion;
                //    crearuser.Comuna = user.Comuna;
                //    crearuser.TelFijo = user.TelFijo;
                //    crearuser.TelMovil = user.TelMovil;
                //    crearuser.Password = user.Password;
                //    crearuser.ConfirmPassword = user.ConfirmPassword;
                //    db.Clientes.Add(crearuser);

                //    try
                //    {
                //        db.SaveChanges();
                //    }
                //    catch (Exception p)
                //    {
                //        ViewBag.error = p.Message;
                //    }
                    
                //}
                //else
                //{
                //    Response.Write("ya existes como usuario");
                //}

                object RegistraUsuario;
                object o1;
                try
                {
                    Microsoft.Dynamics.BusinessConnectorNet.Axapta DynAx = new Microsoft.Dynamics.BusinessConnectorNet.Axapta();
                    System.Net.NetworkCredential jj = new System.Net.NetworkCredential("irojas", "italo123");
                    DynAx.LogonAs("irojas", "yolito.cl", jj, "yod", "", "", "");

                    try
                    {
                        string rutcom = rut1 + "-" + dig1;

                        o1 = DynAx.CallStaticClassMethod("WebPageConection", "FindCustTable", rutcom);
                        string resp = o1.ToString();
                        string[] usuario1 = resp.Split('|');
                        string existe = usuario1[0];

                        if (existe == "FALSE")
                        {
                            RegistraUsuario = DynAx.CallStaticClassMethod("WebPageConection", "CreateCust", rut1 +"-"+ dig1, dig1, user.Nombre + " "+ user.Apellido, user.Email, user.TelFijo, user.TelMovil, user.Comuna, user.Direccion, user.Password);
                            //Response.Write(o.ToString());
                            ViewBag.Message = "Registrado";
                            return View("RegistrationExitosa");
                        }
                        else
                        {
                            ViewBag.error = "Estas registrado en el sistema";
                        }

                    }
                    catch (Exception e)
                    {
                        ViewBag.error =  "[15]" +e.Message;
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.error=("ERROR: " + ex.Message);
                }

            }
            else
            {
                ModelState.AddModelError("", "Formulario no válido");

            }
            return View();
           
        }

        

       
    }
}
