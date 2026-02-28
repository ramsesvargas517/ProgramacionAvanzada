using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaG8.Models
{
    /// <summary>
    /// Modelo que mapea la tabla Producto de la BD Frijolito.
    /// Tabla: Producto (producto_id, sku, nombre, descripcion, precio, stock, categoria_id)
    /// </summary>
    public class ProductoModel
    {
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        [StringLength(50)]
        public string Sku { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoriaId { get; set; }

        // Propiedad auxiliar para mostrar nombre de categoría en vistas
        public string CategoriaNombre { get; set; }
    }
}
