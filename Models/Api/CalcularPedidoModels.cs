namespace Pedidos360Proyecto.Models.Api
{
    public class CalcularPedidoLineaRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
    }

    public class CalcularPedidoRequest
    {
        public List<CalcularPedidoLineaRequest> Lineas { get; set; } = new();
    }

    public class CalcularPedidoLineaResponse
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Descuento { get; set; }
        public decimal ImpuestoPorc { get; set; }
        public decimal TotalLinea { get; set; }
        public bool StockSuficiente { get; set; }
    }

    public class CalcularPedidoResponse
    {
        public List<CalcularPedidoLineaResponse> Lineas { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
    }
}
