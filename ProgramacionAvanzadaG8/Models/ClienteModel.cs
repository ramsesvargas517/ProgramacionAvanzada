using System;
using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaG8.Models
{
    /// <summary>
    /// Modelo que mapea la tabla Cliente de la BD Frijolito.
    /// Tabla: Cliente (cliente_id, identificacion, nombre, apellido, telefono, email, direccion, fecha_registro)
    /// </summary>
    public class ClienteModel
    {
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(50)]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
