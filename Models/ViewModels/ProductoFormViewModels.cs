using System.ComponentModel.DataAnnotations;

namespace Pedidos360Proyecto.Models.ViewModels
{
    public class ProductoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria es obligatoria.")]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El impuesto es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100.")]
        [Display(Name = "Impuesto (%)")]
        public decimal ImpuestoPorc { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Imagen")]
        public IFormFile? Imagen { get; set; }
    }

    public class ProductoEditViewModel : ProductoCreateViewModel
    {
        public int Id { get; set; }

        public string ImagenUrlActual { get; set; } = string.Empty;
    }
}
