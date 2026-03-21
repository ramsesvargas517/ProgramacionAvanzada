using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ProgramacionAvanzadaG8.Models
{

    public class CategoriaModel
    {
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "Máximo 255 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Imagen actual")]
        public string Imagen { get; set; }

        [Display(Name = "Subir imagen")]
        public HttpPostedFileBase ImagenFile { get; set; }
    }
}
