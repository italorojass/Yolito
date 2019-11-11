using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaginaDefinitivaYolito.Models
{
    public class EmailCotizacion
    {
        [Required]
        [StringLength(100, ErrorMessage = "Campo Obligatorio")] 
        public string nombre_cotizante { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Campo Obligatorio")] 
        public string apellido_cotizante { get; set; }

        public string telefono_cotizante { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Campo Obligatorio")] 
        public string From { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "Campo Obligatorio")] 
        public string Subject { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Campo Obligatorio")] 
        public string Body { get; set; }
    }
}