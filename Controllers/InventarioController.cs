using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;

namespace Pedidos360Proyecto.Controllers
{
    [Authorize(Roles = "Admin,Operaciones")]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(ApplicationDbContext context, ILogger<InventarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? nombre)
        {
            var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Contains(nombre));
            }

            ViewBag.Nombre = nombre;

            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
            return View(productos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarStock(int productoId, int nuevoStock, string? nombre)
        {
            if (nuevoStock < 0)
            {
                TempData["Error"] = "El stock no puede ser negativo.";
                return RedirectToAction(nameof(Index), new { nombre });
            }

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto is null)
            {
                return NotFound();
            }

            var stockAnterior = producto.Stock;
            producto.Stock = nuevoStock;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock del producto {Id} ajustado de {Anterior} a {Nuevo}.", productoId, stockAnterior, nuevoStock);
            TempData["Success"] = $"Stock de '{producto.Nombre}' actualizado a {nuevoStock}.";
            return RedirectToAction(nameof(Index), new { nombre });
        }
    }
}
