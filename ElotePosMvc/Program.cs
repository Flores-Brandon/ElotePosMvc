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

// --- 🌐 CONFIGURACIÓN DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- CONFIGURACIÓN DE SERVICIOS ---

builder.Services.AddControllers();

// 🔐 Registramos el servicio de hashing
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// 👇👇👇 ¡ESTA ES LA LÍNEA QUE FALTA PARA ARREGLAR EL ERROR! 👇👇👇
builder.Services.AddDistributedMemoryCache();
// 👆👆👆 SIN ESTO, LAS SESIONES NO FUNCIONAN 👆👆👆

// 🧠 Activamos el uso de sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// --- CONFIGURACIÓN DEL PIPELINE ---

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// 🌐 Activar CORS
app.UseCors("PermitirAngular");

// 🧠 Activar Sesión y Auth
app.UseSession();
app.UseAuthorization();

// --- MAPEO DE RUTAS ---
app.MapControllers();

app.Run();