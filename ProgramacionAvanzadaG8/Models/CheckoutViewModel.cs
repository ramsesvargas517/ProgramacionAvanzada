using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaG8.Models
{
    public class CheckoutViewModel
    {
        public List<CarritoItemModel> ItemsCarrito { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string NombreCliente { get; set; }

        [Required]
        [Display(Name = "Apellido")]
        public string ApellidoCliente { get; set; }

        [Required]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Required]
        [Display(Name = "Método de pago")]
        public int MetodoPagoId { get; set; }

        [Display(Name = "Referencia de pago")]
        public string ReferenciaPago { get; set; }
    }
}