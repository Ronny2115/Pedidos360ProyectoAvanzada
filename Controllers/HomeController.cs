using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models;
using Pedidos360Proyecto.Models.ViewModels;

namespace Pedidos360Proyecto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View(new HomeDashboardViewModel());
            }

            var model = new HomeDashboardViewModel
            {
                TotalCategorias = await _context.Categorias.CountAsync(),
                TotalProductosActivos = await _context.Productos.CountAsync(p => p.Activo),
                TotalClientes = await _context.Clientes.CountAsync(),
                TotalPedidos = await _context.Pedidos.CountAsync()
            };

            model.ProductosStockBajo = await _context.Productos
                .Where(p => p.Activo && p.Stock <= model.StockBajoUmbral)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            model.UltimosPedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.Fecha)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
