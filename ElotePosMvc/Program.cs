using Microsoft.EntityFrameworkCore;
using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE BASES DE DATOS ---

var connectionStringCold = builder.Configuration.GetConnectionString("ColdDbConnection");
var connectionStringHot = builder.Configuration.GetConnectionString("HotDbConnection");

builder.Services.AddDbContext<ColdDbContext>(options =>
    options.UseSqlServer(connectionStringCold)
);

builder.Services.AddDbContext<HotDbContext>(options =>
    options.UseMySql(connectionStringHot, ServerVersion.AutoDetect(connectionStringHot))
);

// --- CONFIGURACIÓN DE SERVICIOS ---

builder.Services.AddControllersWithViews();

// 🔐 Registramos el servicio de hashing de contraseñas
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// 🧠 Activamos el uso de sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // duración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- CONSTRUIR LA APLICACIÓN ---
var app = builder.Build();

// --- CONFIGURACIÓN DEL PIPELINE ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🧠 Activar las sesiones antes de autorización
app.UseSession();

app.UseAuthorization();

// --- CONFIGURAR RUTA PRINCIPAL ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}"
);

app.Run();
