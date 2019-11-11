using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Dynamics.BusinessConnectorNet;

namespace PaginaDefinitivaYolito.Models
{
    public class ProductosModel
    {
        public int IdProd { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Clasif1 { get; set; }
        public string Clasif2 { get; set; }
        public string Clasif3 { get; set; }
        public string ConfigId { get; set; }
        public string InventSizeId { get; set; }
        public string InventColorId { get; set; }
        public string ItemBarCode { get; set; }
        public int Price { get; set; }
        public string Photo { get; set; }
        public string HighLights { get; set; }
        public string ICat3 { get; set; }
        public string Stock { get; set; }
        public string brandInternet { get; set; }
        public long RefRecId { get; set; }
        public string UnitWeb { get; set; }
        public double Factor { get; set; }
        public int Orden { get; set; }

        
        

    }



    
}