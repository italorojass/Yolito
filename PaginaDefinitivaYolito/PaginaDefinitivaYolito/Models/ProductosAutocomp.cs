using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class ProductosAutocomp
    {

        private PaginaWebEntities1 db = new PaginaWebEntities1();

        public List<string> Search(string name)
        {
            return db.Productos.Where(p => p.ItemName.StartsWith(name)).Select(p => p.ItemName).ToList();
        }
    }
}