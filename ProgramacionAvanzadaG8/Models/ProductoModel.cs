using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ProgramacionAvanzadaG8.Models
{

    public class ProductoModel
    {
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre del Producto")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "Máximo 255 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Display(Name = "Precio (₡)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, 99999, ErrorMessage = "Stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }


        public string CategoriaNombre { get; set; }

        [Display(Name = "Imagen actual")]
        public string Imagen { get; set; }

        [Display(Name = "Subir imagen")]
        public HttpPostedFileBase ImagenFile { get; set; }

        [Required(ErrorMessage = "Debe seleccionar a quién va dirigido.")]
        [Display(Name = "Dirigido a")]
        public string Genero { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}
