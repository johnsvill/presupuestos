using Microsoft.AspNetCore.Identity;
using Presupuestos.Models;
using Presupuestos.Servicios;
using Presupuestos.Servicios.Categorias;
using Presupuestos.Servicios.Cuentas;
using Presupuestos.Servicios.Reportes;
using Presupuestos.Servicios.Transacciones;
using Presupuestos.Servicios.Usuarios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<ICategorias, Categorias>();
builder.Services.AddTransient<ITransacciones, Transacciones>();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();

builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
