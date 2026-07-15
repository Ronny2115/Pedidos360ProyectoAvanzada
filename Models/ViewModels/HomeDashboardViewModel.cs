namespace Pedidos360Proyecto.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public int TotalCategorias { get; set; }
        public int TotalProductosActivos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalPedidos { get; set; }

        public int StockBajoUmbral { get; set; } = 10;
        public List<Producto> ProductosStockBajo { get; set; } = new();

        public List<Pedido> UltimosPedidos { get; set; } = new();
    }
}
