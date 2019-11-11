using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class Nivel1Model
    {
        private PaginaWebEntities1 db = new PaginaWebEntities1();

        public List<string> getNivel1(string IdNivel1)
        {
            return db.Nivel1.Where(p => p.ICat1 == IdNivel1).Select(m => m.Name).ToList();
        }

        public Array getNivel2(string getIdnivel1)
        {
            var queryGetNivel1 =from n1 in db.Nivel1
                                join n2 in db.Nivel2 on n1.ICat1 equals n2.ICat1
                                where n1.ICat1 == getIdnivel1
                                select n2;

            return queryGetNivel1.ToArray();
        } 


    }
}