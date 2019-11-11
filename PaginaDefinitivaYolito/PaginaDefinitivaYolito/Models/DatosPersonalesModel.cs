using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace PaginaDefinitivaYolito.Models
{
    public class DatosPersonalesModel
    {
        [Required(ErrorMessage = "El Rut es obligatorio.")]
        public string rut { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string NumeroContacto { get; set; }

        [Required(ErrorMessage = "Comuna es obligatoria")]
        public string Comuna { get; set; }

        [Required(ErrorMessage = "Dirección obligatoria")]
        public string Direccion { get; set;}

        public int TelFijo { get; set; }
        public int TelMovil { get; set; }

        [Required(ErrorMessage = "Contraseña obligatoria.")]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar la nueva contraseña")]
        [Compare("Password", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
        
    }
}