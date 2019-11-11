using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class CarritoModel
    {
        public int Id { get; set; }
        public int NPedido { get; set; }
        public string ItemBarCode { get; set; }
        public string ItemName { get; set; }
        public int Cantidad { get; set; }
        public string Precio { get; set; }
        public int Subtotal { get; set; }
        public string Photo { get; set; }
    }
}