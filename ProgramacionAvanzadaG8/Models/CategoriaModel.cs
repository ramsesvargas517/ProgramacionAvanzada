using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaG8.Models
{
    /// <summary>
    /// Modelo que mapea la tabla Categoria de la BD Frijolito.
    /// Tabla: Categoria (categoria_id, nombre, descripcion)
    /// </summary>
    public class CategoriaModel
    {
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El nombre de categoría es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }
    }
}
