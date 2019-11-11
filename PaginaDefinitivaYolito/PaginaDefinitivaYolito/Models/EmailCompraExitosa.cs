using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class EmailCompraExitosa
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string numero_tarjeta { get; set; }
        public string fecha_pago { get; set; }
        public string hora_pago { get; set; }
        public string tipo_pago { get; set; }
        public string numero_orden_compra { get; set; }
        public string monto { get; set; }
        public string usuario { get; set; }
        public string codigo { get; set; }
        public string cuotas { get; set; }
        public string tipo_cuotan { get; set; }

    }
}