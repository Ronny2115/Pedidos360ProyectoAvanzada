using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models;
using Pedidos360Proyecto.Models.ViewModels;

namespace Pedidos360Proyecto.Controllers
{
    [Authorize(Roles = "Admin,Ventas,Operaciones")]
    public class ProductosController : Controller
    {
        private const int TamanoPaginaDefault = 6;
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProductosController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? nombre, int? categoriaId, int pagina = 1)
        {
            var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Contains(nombre));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            var totalRegistros = await query.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)TamanoPaginaDefault));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * TamanoPaginaDefault)
                .Take(TamanoPaginaDefault)
                .ToListAsync();

            var viewModel = new ProductoIndexViewModel
            {
                Productos = productos,
                Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
                Nombre = nombre,
                CategoriaId = categoriaId,
                Pagina = pagina,
                TamanoPagina = TamanoPaginaDefault,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
            if (producto is null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await CargarCategoriasAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoCreateViewModel model)
        {
            if (model.Imagen is null || model.Imagen.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Imagen), "La imagen es obligatoria.");
            }
            else if (!ExtensionesPermitidas.Contains(Path.GetExtension(model.Imagen.FileName).ToLowerInvariant()))
            {
                ModelState.AddModelError(nameof(model.Imagen), "Formato de imagen no soportado.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model.CategoriaId);
                return View(model);
            }

            var producto = new Producto
            {
                Nombre = model.Nombre,
                CategoriaId = model.CategoriaId,
                Precio = model.Precio,
                ImpuestoPorc = model.ImpuestoPorc,
                Stock = model.Stock,
                Activo = model.Activo,
                ImagenUrl = await GuardarImagenAsync(model.Imagen!)
            };

            _context.Add(producto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Producto creado: {Nombre}", producto.Nombre);
            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            var model = new ProductoEditViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CategoriaId = producto.CategoriaId,
                Precio = producto.Precio,
                ImpuestoPorc = producto.ImpuestoPorc,
                Stock = producto.Stock,
                Activo = producto.Activo,
                ImagenUrlActual = producto.ImagenUrl
            };

            await CargarCategoriasAsync(producto.CategoriaId);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (model.Imagen is not null && model.Imagen.Length > 0 &&
                !ExtensionesPermitidas.Contains(Path.GetExtension(model.Imagen.FileName).ToLowerInvariant()))
            {
                ModelState.AddModelError(nameof(model.Imagen), "Formato de imagen no soportado.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model.CategoriaId);
                return View(model);
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            producto.Nombre = model.Nombre;
            producto.CategoriaId = model.CategoriaId;
            producto.Precio = model.Precio;
            producto.ImpuestoPorc = model.ImpuestoPorc;
            producto.Stock = model.Stock;
            producto.Activo = model.Activo;

            if (model.Imagen is not null && model.Imagen.Length > 0)
            {
                producto.ImagenUrl = await GuardarImagenAsync(model.Imagen);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Producto {Id} actualizado.", id);
            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
            if (producto is null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var tieneDetalles = await _context.PedidoDetalles.AnyAsync(d => d.ProductoId == id);
            if (tieneDetalles)
            {
                TempData["Error"] = "No se puede eliminar el producto porque tiene pedidos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Producto {Id} eliminado.", id);
            TempData["Success"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategoriasAsync(int? categoriaSeleccionada = null)
        {
            ViewBag.Categorias = new SelectList(
                await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
                "Id", "Nombre", categoriaSeleccionada);
        }

        private async Task<string> GuardarImagenAsync(IFormFile imagen)
        {
            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var carpeta = Path.Combine(_environment.WebRootPath, "images", "productos");
            Directory.CreateDirectory(carpeta);
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return $"/images/productos/{nombreArchivo}";
        }
    }
}
