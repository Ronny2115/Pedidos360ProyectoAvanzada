using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Pedidos360Proyecto.Data;
using Pedidos360Proyecto.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Errors/AccesoDenegado";
});

builder.Services.AddScoped<PedidoCalculoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errors/500");
    app.UseHsts();
}

// Cultura fija en-US para que los montos se muestren como $25.00
// (signo de dolar, punto decimal y coma de miles) de forma uniforme en
// toda la aplicacion, y para que el binding de decimales use punto decimal.
var culturaMontos = new CultureInfo("en-US");
var opcionesLocalizacion = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaMontos),
    SupportedCultures = new[] { culturaMontos },
    SupportedUICultures = new[] { culturaMontos }
};
app.UseRequestLocalization(opcionesLocalizacion);

app.UseStatusCodePagesWithReExecute("/Errors/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.Run();
