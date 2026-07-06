using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models;
using Pedidos360Proyecto.Models.Api;
using Pedidos360Proyecto.Models.ViewModels;
using Pedidos360Proyecto.Services;

namespace Pedidos360Proyecto.Controllers
{
    [Authorize(Roles = "Admin,Ventas,Operaciones")]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PedidoCalculoService _calculoService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(ApplicationDbContext context, PedidoCalculoService calculoService, ILogger<PedidosController> logger)
        {
            _context = context;
            _calculoService = calculoService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(pedidos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido is null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        [Authorize(Roles = "Admin,Ventas")]
        public async Task<IActionResult> Create()
        {
            await CargarClientesAsync();
            return View(new PedidoCreateViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Ventas")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoCreateViewModel model)
        {
            model.Lineas ??= new();
            if (model.Lineas.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto al pedido.");
            }

            var clienteExiste = model.ClienteId > 0 && await _context.Clientes.AnyAsync(c => c.Id == model.ClienteId);
            if (!clienteExiste)
            {
                ModelState.AddModelError(nameof(model.ClienteId), "Debe seleccionar un cliente válido.");
            }

            if (!ModelState.IsValid)
            {
                await CargarClientesAsync(model.ClienteId);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var lineasRequest = model.Lineas.Select(l => new CalcularPedidoLineaRequest
            {
                ProductoId = l.ProductoId,
                Cantidad = l.Cantidad,
                Descuento = l.Descuento
            });

            var calculo = await _calculoService.CalcularAsync(lineasRequest);

            var lineasSinStock = calculo.Lineas.Where(l => !l.StockSuficiente).ToList();
            if (lineasSinStock.Count > 0)
            {
                foreach (var linea in lineasSinStock)
                {
                    ModelState.AddModelError(string.Empty, $"Stock insuficiente para el producto '{linea.Nombre}'.");
                }
                await transaction.RollbackAsync();
                await CargarClientesAsync(model.ClienteId);
                return View(model);
            }

            var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var pedido = new Pedido
            {
                ClienteId = model.ClienteId,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                Subtotal = calculo.Subtotal,
                Impuestos = calculo.Impuestos,
                Total = calculo.Total,
                Estado = EstadoPedido.Confirmado
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            foreach (var linea in calculo.Lineas)
            {
                var producto = await _context.Productos.FindAsync(linea.ProductoId);
                if (producto is null || producto.Stock < linea.Cantidad)
                {
                    ModelState.AddModelError(string.Empty, $"Stock insuficiente para el producto '{linea.Nombre}'.");
                    await transaction.RollbackAsync();
                    await CargarClientesAsync(model.ClienteId);
                    return View(model);
                }

                producto.Stock -= linea.Cantidad;

                _context.PedidoDetalles.Add(new PedidoDetalle
                {
                    PedidoId = pedido.Id,
                    ProductoId = linea.ProductoId,
                    Cantidad = linea.Cantidad,
                    PrecioUnit = linea.PrecioUnit,
                    Descuento = linea.Descuento,
                    ImpuestoPorc = linea.ImpuestoPorc,
                    TotalLinea = linea.TotalLinea
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Pedido {Id} confirmado por el usuario {UsuarioId} para el cliente {ClienteId}.", pedido.Id, usuarioId, pedido.ClienteId);
            TempData["Success"] = "Pedido confirmado correctamente.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        private async Task CargarClientesAsync(int? clienteSeleccionado = null)
        {
            ViewBag.Clientes = new SelectList(
                await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync(),
                "Id", "Nombre", clienteSeleccionado);
        }
    }
}
