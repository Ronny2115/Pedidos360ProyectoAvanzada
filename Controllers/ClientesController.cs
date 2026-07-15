using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Models;

namespace Pedidos360Proyecto.Controllers
{
    [Authorize(Roles = "Admin,Ventas")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(ApplicationDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? busqueda)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(c => c.Nombre.Contains(busqueda) || c.Cedula.Contains(busqueda));
            }

            ViewBag.Busqueda = busqueda;

            var clientes = await query.OrderBy(c => c.Nombre).ToListAsync();
            return View(clientes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            if (cliente is null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Cedula,Correo,Telefono,Direccion")] Cliente cliente)
        {
            if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
            {
                ModelState.AddModelError(nameof(cliente.Cedula), "Ya existe un cliente con esta cedula.");
            }

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            _context.Add(cliente);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cliente creado: {Nombre}", cliente.Nombre);
            TempData["Success"] = "Cliente creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cedula,Correo,Telefono,Direccion")] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.Id != id))
            {
                ModelState.AddModelError(nameof(cliente.Cedula), "Ya existe un cliente con esta cedula.");
            }

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cliente {Id} actualizado.", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Clientes.AnyAsync(c => c.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            TempData["Success"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            if (cliente is null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var tienePedidos = await _context.Pedidos.AnyAsync(p => p.ClienteId == id);
            if (tienePedidos)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene pedidos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cliente {Id} eliminado.", id);
            TempData["Success"] = "Cliente eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
