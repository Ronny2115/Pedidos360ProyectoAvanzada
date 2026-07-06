# Pedidos360

Sistema web para la administración de productos, categorías, clientes, inventario y pedidos, desarrollado con **ASP.NET Core MVC (.NET 10)**, **Entity Framework Core (Code First)**, **SQL Server**, **ASP.NET Core Identity**, **Bootstrap 5** y **jQuery/AJAX**.

## Características principales

- CRUD completo de Categorías, Productos y Clientes, con validaciones de servidor y cliente.
- Módulo de Pedidos con un "carrito" interactivo: búsqueda de productos en tiempo real, cálculo automático de subtotal/impuestos/total, control de stock y confirmación transaccional.
- API REST interna para el módulo de pedidos:
  - `GET /api/productos/buscar?q=texto` — búsqueda de hasta 10 productos activos.
  - `POST /api/pedidos/calcular` — recálculo autoritativo de totales a partir de las líneas del pedido.
- Autenticación con ASP.NET Core Identity y autorización por roles: **Admin**, **Ventas**, **Operaciones**.
- Módulo de Inventario para ajustar stock sin exponer el resto de los campos del producto.
- Páginas de error personalizadas (404, 500, 403) y logging de las operaciones críticas.
- Datos semilla (seed) para poder probar el sistema de inmediato.

## Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/) o superior.
- Una instancia de SQL Server accesible (por defecto el proyecto apunta a una instancia local llamada `localhost`; ver más abajo cómo cambiarla).
- Herramienta `dotnet-ef` (se instala como se indica más abajo).

## Instalación

1. Clonar/descargar el repositorio y ubicarse en la carpeta del proyecto.
2. Restaurar los paquetes NuGet:

   ```bash
   dotnet restore
   ```

3. Instalar la herramienta de EF Core (si no la tienes instalada globalmente):

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Revisar/ajustar la cadena de conexión en `appsettings.json` según tu servidor SQL Server:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=Pedidos360Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

   La cadena por defecto usa autenticación de Windows (`Trusted_Connection=True`) contra la instancia `localhost`. `TrustServerCertificate=True` evita errores de certificado SSL en entornos de desarrollo. Si usas SQL Server Authentication, reemplaza `Trusted_Connection=True` por `User Id=...;Password=...`. Si prefieres LocalDB, usa `Server=(localdb)\\mssqllocaldb;...` en su lugar.

## Base de datos (Migrations)

El proyecto usa Entity Framework Core Code First. Para crear la base de datos y aplicar el esquema:

```bash
dotnet ef database update
```

Esto crea la base `Pedidos360Db` en la instancia configurada (`localhost` por defecto) con todas las tablas (Categorias, Productos, Clientes, Pedidos, PedidoDetalles, AspNet*).

> Si necesitas regenerar la base desde cero: `dotnet ef database drop --force` seguido de `dotnet ef database update`.

## Cómo ejecutar el proyecto

```bash
dotnet run
```

Al iniciar, la aplicación aplica automáticamente las migraciones pendientes y ejecuta el *seed* de datos (roles, usuarios, categorías, productos y clientes) si la base de datos está vacía. Luego abre la URL indicada en la consola (por ejemplo `http://localhost:5288`).

## Roles y usuarios de prueba

Se crean automáticamente 3 roles y un usuario por cada uno:

| Rol         | Correo                        | Contraseña          | Permisos principales                                                                 |
|-------------|-------------------------------|----------------------|----------------------------------------------------------------------------------------|
| Admin       | admin@pedidos360.com          | Admin123$           | Acceso completo: Categorías, Productos, Clientes, Pedidos e Inventario.               |
| Ventas      | ventas@pedidos360.com         | Ventas123$          | Crear y consultar pedidos, consultar productos (sin editar), CRUD de clientes.       |
| Operaciones | operaciones@pedidos360.com    | Operaciones123$     | Consultar productos y pedidos, administrar inventario (ajuste de stock).             |

## Datos semilla

Al primer arranque se crean:

- 2 categorías (Electrónica, Alimentos).
- 12 productos distribuidos entre ambas categorías.
- 8 clientes de ejemplo.
- Los 3 roles y usuarios descritos arriba.

## Estructura del proyecto

```
Controllers/            Controladores MVC (Categorias, Productos, Clientes, Pedidos, Inventario, Errors)
Controllers/Api/         Controladores de API (productos/buscar, pedidos/calcular)
Data/                    ApplicationDbContext y DbInitializer (seed)
Models/                  Entidades del dominio
Models/ViewModels/       ViewModels para formularios y listados
Models/Api/              DTOs de la API
Services/                Lógica de cálculo de pedidos (PedidoCalculoService)
Views/                   Vistas Razor (Bootstrap 5)
wwwroot/js/pedido-builder.js   Lógica del carrito de pedidos (búsqueda AJAX + recálculo)
Migrations/              Migraciones de Entity Framework Core
```

## Notas de seguridad

- Todos los controladores MVC y de API requieren autenticación (`[Authorize]`); las acciones sensibles restringen además por rol.
- Los totales de cada pedido (subtotal, impuestos, total) se calculan y validan en el servidor — nunca se confía en los valores enviados por el cliente — y se almacenan de forma permanente para fines de auditoría.
- El descuento de stock ocurre dentro de una transacción de base de datos que verifica disponibilidad antes de confirmar, evitando inventario negativo.
