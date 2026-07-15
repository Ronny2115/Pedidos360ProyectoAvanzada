using System.ComponentModel.DataAnnotations;

namespace Pedidos360Proyecto.Models.ViewModels
{
    public class PedidoLineaInputModel
    {
        [Required]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public decimal Descuento { get; set; }
    }

    public class PedidoCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        public List<PedidoLineaInputModel> Lineas { get; set; } = new();
    }
}
