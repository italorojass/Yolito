using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class EnviaCVModel
    {
        public string rut { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string TelefonoFijo { get; set; }
        public string TelefonoMovil { get; set; }
        public string Dirección { get; set; }
        
        public HttpPostedFileBase file { get; set; }
    }
}