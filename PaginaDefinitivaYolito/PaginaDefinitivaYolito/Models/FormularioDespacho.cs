using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class FormularioDespacho
    {
        [Required]
        public string Nombre { get; set; }
    }
}