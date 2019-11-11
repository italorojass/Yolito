using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaginaDefinitivaYolito.Models;

namespace PaginaDefinitivaYolito.Controllers
{
    public class CheckoutController : Controller
    {
        //
        // GET: /Checkout/
        private PaginaWebEntities1 db = new PaginaWebEntities1();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TipoPago()
        {

            return View("TipoPago", db.DetalleCarro);
        }

        [HttpPost]
        public ActionResult TipoPago(string select)
        {
            if (ModelState.IsValid)
            {

            }
            else
            {
                Response.Write("MODELSTATE NO VALIDO");
            }
            //validar stock
            //recibe form


            return View(db.DetalleCarro);
        } 

    }
}
