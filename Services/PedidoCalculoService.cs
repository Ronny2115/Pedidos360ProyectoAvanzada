using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models.Api;

namespace Pedidos360Proyecto.Services
{
    public class PedidoCalculoService
    {
        private readonly ApplicationDbContext _context;

        public PedidoCalculoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CalcularPedidoResponse> CalcularAsync(IEnumerable<CalcularPedidoLineaRequest> lineas)
        {
            var response = new CalcularPedidoResponse();
            var lineasList = lineas.ToList();
            var productoIds = lineasList.Select(l => l.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var linea in lineasList)
            {
                if (!productos.TryGetValue(linea.ProductoId, out var producto))
                {
                    continue;
                }

                var cantidad = Math.Max(0, linea.Cantidad);
                var descuento = Math.Clamp(linea.Descuento, 0, 100);

                var subtotalLinea = producto.Precio * cantidad * (1 - descuento / 100m);
                var impuestoLinea = subtotalLinea * (producto.ImpuestoPorc / 100m);

                response.Lineas.Add(new CalcularPedidoLineaResponse
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnit = producto.Precio,
                    Descuento = descuento,
                    ImpuestoPorc = producto.ImpuestoPorc,
                    TotalLinea = Math.Round(subtotalLinea + impuestoLinea, 2),
                    StockSuficiente = producto.Stock >= cantidad
                });

                response.Subtotal += subtotalLinea;
                response.Impuestos += impuestoLinea;
            }

            response.Subtotal = Math.Round(response.Subtotal, 2);
            response.Impuestos = Math.Round(response.Impuestos, 2);
            response.Total = response.Subtotal + response.Impuestos;

            return response;
        }
    }
}
