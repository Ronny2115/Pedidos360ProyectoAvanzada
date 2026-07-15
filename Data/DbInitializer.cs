using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Models;

namespace Pedidos360Proyecto.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            await SeedRolesAndUsersAsync(services);
            await SeedCatalogAsync(context);
            await SeedPedidosAsync(context, services);
        }

        private static async Task SeedRolesAndUsersAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { "Admin", "Ventas", "Operaciones" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var usuarios = new (string Email, string Password, string Role)[]
            {
                ("admin@pedidos360.com", "Admin123$", "Admin"),
                ("ventas@pedidos360.com", "Ventas123$", "Ventas"),
                ("operaciones@pedidos360.com", "Operaciones123$", "Operaciones"),
            };

            foreach (var (email, password, role) in usuarios)
            {
                if (await userManager.FindByEmailAsync(email) is not null)
                {
                    continue;
                }

                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedCatalogAsync(ApplicationDbContext context)
        {
            if (!await context.Categorias.AnyAsync())
            {
                var pasteles = new Categoria { Nombre = "Pasteles y Tortas" };
                var individual = new Categoria { Nombre = "Reposteria Individual" };
                context.Categorias.AddRange(pasteles, individual);
                await context.SaveChangesAsync();

                context.Productos.AddRange(
                    new Producto { Nombre = "Torta de Chocolate", CategoriaId = pasteles.Id, Precio = 28.50m, ImpuestoPorc = 13, Stock = 12, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Torta Tres Leches", CategoriaId = pasteles.Id, Precio = 26.00m, ImpuestoPorc = 13, Stock = 10, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Cheesecake de Fresa", CategoriaId = pasteles.Id, Precio = 32.00m, ImpuestoPorc = 13, Stock = 8, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Pie de Limon", CategoriaId = pasteles.Id, Precio = 24.75m, ImpuestoPorc = 13, Stock = 15, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Torta Red Velvet", CategoriaId = pasteles.Id, Precio = 30.00m, ImpuestoPorc = 13, Stock = 9, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Torta de Zanahoria", CategoriaId = pasteles.Id, Precio = 25.50m, ImpuestoPorc = 13, Stock = 11, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Torta Selva Negra", CategoriaId = pasteles.Id, Precio = 29.90m, ImpuestoPorc = 13, Stock = 7, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Croissant de Almendra", CategoriaId = individual.Id, Precio = 3.25m, ImpuestoPorc = 8, Stock = 60, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Muffin de Arandanos", CategoriaId = individual.Id, Precio = 2.80m, ImpuestoPorc = 8, Stock = 70, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Alfajor de Dulce de Leche", CategoriaId = individual.Id, Precio = 2.20m, ImpuestoPorc = 8, Stock = 90, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Brownie de Chocolate", CategoriaId = individual.Id, Precio = 3.00m, ImpuestoPorc = 8, Stock = 65, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Cupcake de Vainilla", CategoriaId = individual.Id, Precio = 2.50m, ImpuestoPorc = 8, Stock = 80, ImagenUrl = "/images/placeholder.svg", Activo = true }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Clientes.AnyAsync())
            {
                context.Clientes.AddRange(
                    new Cliente { Nombre = "Juan Perez", Cedula = "001-1111111-1", Correo = "juan.perez@example.com", Telefono = "809-555-0001", Direccion = "Calle Primera #1, Santo Domingo" },
                    new Cliente { Nombre = "Maria Gomez", Cedula = "001-2222222-2", Correo = "maria.gomez@example.com", Telefono = "809-555-0002", Direccion = "Av. Central #22, Santiago" },
                    new Cliente { Nombre = "Carlos Rodriguez", Cedula = "001-3333333-3", Correo = "carlos.rodriguez@example.com", Telefono = "809-555-0003", Direccion = "Calle Duarte #33, La Vega" },
                    new Cliente { Nombre = "Ana Martinez", Cedula = "001-4444444-4", Correo = "ana.martinez@example.com", Telefono = "809-555-0004", Direccion = "Av. Independencia #44, San Cristobal" },
                    new Cliente { Nombre = "Luis Fernandez", Cedula = "001-5555555-5", Correo = "luis.fernandez@example.com", Telefono = "809-555-0005", Direccion = "Calle Sol #55, Puerto Plata" },
                    new Cliente { Nombre = "Laura Sanchez", Cedula = "001-6666666-6", Correo = "laura.sanchez@example.com", Telefono = "809-555-0006", Direccion = "Calle Luna #66, Moca" },
                    new Cliente { Nombre = "Pedro Ramirez", Cedula = "001-7777777-7", Correo = "pedro.ramirez@example.com", Telefono = "809-555-0007", Direccion = "Av. Bolivar #77, San Pedro" },
                    new Cliente { Nombre = "Sofia Torres", Cedula = "001-8888888-8", Correo = "sofia.torres@example.com", Telefono = "809-555-0008", Direccion = "Calle Mella #88, Higuey" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPedidosAsync(ApplicationDbContext context, IServiceProvider services)
        {
            if (await context.Pedidos.AnyAsync())
            {
                return;
            }

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var usuarioVentas = await userManager.FindByEmailAsync("ventas@pedidos360.com");
            if (usuarioVentas is null)
            {
                return;
            }

            var clientes = await context.Clientes.OrderBy(c => c.Id).Take(2).ToListAsync();
            var tortaChocolate = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "Torta de Chocolate");
            var croissant = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "Croissant de Almendra");
            var cheesecake = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "Cheesecake de Fresa");

            if (clientes.Count < 2 || tortaChocolate is null || croissant is null || cheesecake is null)
            {
                return;
            }

            decimal Linea(decimal precio, int cantidad, decimal impuestoPorc) =>
                Math.Round(precio * cantidad * (1 + impuestoPorc / 100m), 2);

            var pedido1Detalle1 = new PedidoDetalle
            {
                ProductoId = tortaChocolate.Id,
                Cantidad = 2,
                PrecioUnit = tortaChocolate.Precio,
                Descuento = 0,
                ImpuestoPorc = tortaChocolate.ImpuestoPorc,
                TotalLinea = Linea(tortaChocolate.Precio, 2, tortaChocolate.ImpuestoPorc)
            };
            var pedido1Detalle2 = new PedidoDetalle
            {
                ProductoId = croissant.Id,
                Cantidad = 6,
                PrecioUnit = croissant.Precio,
                Descuento = 0,
                ImpuestoPorc = croissant.ImpuestoPorc,
                TotalLinea = Linea(croissant.Precio, 6, croissant.ImpuestoPorc)
            };
            var pedido1Subtotal = tortaChocolate.Precio * 2 + croissant.Precio * 6;
            var pedido1Impuestos = pedido1Detalle1.TotalLinea - (tortaChocolate.Precio * 2) + pedido1Detalle2.TotalLinea - (croissant.Precio * 6);

            var pedido1 = new Pedido
            {
                ClienteId = clientes[0].Id,
                UsuarioId = usuarioVentas.Id,
                Fecha = DateTime.Now,
                Subtotal = Math.Round(pedido1Subtotal, 2),
                Impuestos = Math.Round(pedido1Impuestos, 2),
                Total = pedido1Detalle1.TotalLinea + pedido1Detalle2.TotalLinea,
                Estado = EstadoPedido.Confirmado,
                Detalles = new List<PedidoDetalle> { pedido1Detalle1, pedido1Detalle2 }
            };

            var pedido2Detalle = new PedidoDetalle
            {
                ProductoId = cheesecake.Id,
                Cantidad = 1,
                PrecioUnit = cheesecake.Precio,
                Descuento = 0,
                ImpuestoPorc = cheesecake.ImpuestoPorc,
                TotalLinea = Linea(cheesecake.Precio, 1, cheesecake.ImpuestoPorc)
            };
            var pedido2 = new Pedido
            {
                ClienteId = clientes[1].Id,
                UsuarioId = usuarioVentas.Id,
                Fecha = DateTime.Now,
                Subtotal = cheesecake.Precio,
                Impuestos = Math.Round(pedido2Detalle.TotalLinea - cheesecake.Precio, 2),
                Total = pedido2Detalle.TotalLinea,
                Estado = EstadoPedido.Confirmado,
                Detalles = new List<PedidoDetalle> { pedido2Detalle }
            };

            context.Pedidos.AddRange(pedido1, pedido2);
            await context.SaveChangesAsync();
        }
    }
}