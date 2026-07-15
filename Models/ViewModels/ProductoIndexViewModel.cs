namespace Pedidos360Proyecto.Models.ViewModels
{
    public class ProductoIndexViewModel
    {
        public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();

        public IEnumerable<Categoria> Categorias { get; set; } = new List<Categoria>();

        public string? Nombre { get; set; }

        public int? CategoriaId { get; set; }

        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 6;

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }
    }
}
