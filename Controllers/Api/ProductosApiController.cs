using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models.Api;

namespace Pedidos360Proyecto.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/productos")]
    public class ProductosApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ProductoBusquedaDto>>> Buscar([FromQuery] string? q)
        {
            var query = _context.Productos.Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Nombre.Contains(q));
            }

            var productos = await query
                .OrderBy(p => p.Nombre)
                .Take(10)
                .Select(p => new ProductoBusquedaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Impuesto = p.ImpuestoPorc,
                    Stock = p.Stock
                })
                .ToListAsync();

            return Ok(productos);
        }
    }
}
