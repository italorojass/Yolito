using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class ProductModel
    {
        public List<Productos> findAll()
        {
            using(PaginaWebEntities1 db = new PaginaWebEntities1())
            {
                List<Productos> listProducts = db.Productos.ToList();
                return listProducts;
            }
        }

        public List<string> Search(string name)
        {
            return this.findAll().Where(p => p.ItemName.StartsWith(name,
                StringComparison.OrdinalIgnoreCase)).Select(p => p.ItemName + "" + p.brandInternet).ToList();
        }
    }
}