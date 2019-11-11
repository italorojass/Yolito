using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaginaDefinitivaYolito.Models
{
    public class Resetpw
    {
        
        [Required]
        [StringLength(100, ErrorMessage = "La contraseña debe ser de al menos 4 digitos", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar la nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas NO coinciden")]
        public string ConfirmPassword { get; set; }
    }
}