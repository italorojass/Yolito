using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaginaDefinitivaYolito.Models
{
    public class ResetPassword
    {
        [Required]
        [Display(Name = "Rut con guión")]
        public string rut { get; set; }
        
    }
}