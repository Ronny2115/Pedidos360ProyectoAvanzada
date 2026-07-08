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
                var electronica = new Categoria { Nombre = "Electronica" };
                var alimentos = new Categoria { Nombre = "Alimentos" };
                context.Categorias.AddRange(electronica, alimentos);
                await context.SaveChangesAsync();

                context.Productos.AddRange(
                    new Producto { Nombre = "Audifonos Bluetooth", CategoriaId = electronica.Id, Precio = 45.99m, ImpuestoPorc = 13, Stock = 30, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Teclado Mecanico", CategoriaId = electronica.Id, Precio = 89.50m, ImpuestoPorc = 13, Stock = 20, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Mouse Inalambrico", CategoriaId = electronica.Id, Precio = 25.00m, ImpuestoPorc = 13, Stock = 50, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Monitor 24 pulgadas", CategoriaId = electronica.Id, Precio = 210.00m, ImpuestoPorc = 13, Stock = 15, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Webcam HD", CategoriaId = electronica.Id, Precio = 38.75m, ImpuestoPorc = 13, Stock = 25, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Disco Externo 1TB", CategoriaId = electronica.Id, Precio = 65.00m, ImpuestoPorc = 13, Stock = 18, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Cargador USB-C", CategoriaId = electronica.Id, Precio = 15.25m, ImpuestoPorc = 13, Stock = 60, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Cafe Molido 500g", CategoriaId = alimentos.Id, Precio = 6.50m, ImpuestoPorc = 8, Stock = 100, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Aceite de Oliva 1L", CategoriaId = alimentos.Id, Precio = 9.80m, ImpuestoPorc = 8, Stock = 70, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Arroz 2kg", CategoriaId = alimentos.Id, Precio = 3.20m, ImpuestoPorc = 8, Stock = 120, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Pasta Italiana 500g", CategoriaId = alimentos.Id, Precio = 2.75m, ImpuestoPorc = 8, Stock = 90, ImagenUrl = "/images/placeholder.svg", Activo = true },
                    new Producto { Nombre = "Chocolate Amargo 100g", CategoriaId = alimentos.Id, Precio = 4.10m, ImpuestoPorc = 8, Stock = 80, ImagenUrl = "/images/placeholder.svg", Activo = true }
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
    }
}
