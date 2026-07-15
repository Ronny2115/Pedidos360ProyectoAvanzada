namespace Pedidos360Proyecto.Models.Api
{
    public class ProductoBusquedaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Impuesto { get; set; }
        public int Stock { get; set; }
    }
}
