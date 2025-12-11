using ElotePosMvc.Data;
using ElotePosMvc.Models; // Para IPasswordHasher<Usuario>
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // Necesario para JSON

var builder = WebApplication.CreateBuilder(args);
// --- 1. CONFIGURACIÓN DE CONTEXTOS DE BASES DE DATOS ---

// A. ColdDbContext: SQL Server local (Catálogo)
builder.Services.AddDbContext<ColdDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ColdDbConnection")));

// B. HotDbContext: SQL Server con Linked Server (Transaccional)
// ⚠️ CAMBIO AQUÍ: Usamos "ColdDbConnection" también, porque ambos entran por SQL Server.
builder.Services.AddDbContext<HotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ColdDbConnection")));

// --- 2. SERVICIOS PRINCIPALES ---

// Agregar controladores y configuración de JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Esto ayuda a manejar referencias circulares y mejorar la serialización
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Configuración para usar números decimales sin pérdida de precisión si es necesario
        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.AllowReadingFromString |
            JsonNumberHandling.WriteAsString;
    });

// Agregamos el PasswordHasher para el hashing de contraseñas de Identity
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();


// --- 3. CONFIGURACIÓN DE SESIÓN (Para la Autenticación y Auditoría) ---
builder.Services.AddDistributedMemoryCache();
// Habilitar el uso de sesiones en el servidor
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // Duración de la sesión: 8 horas
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// --- 4. CONFIGURACIÓN DE CORS (Para conectar con Angular) ---

var MiCors = "MiCorsPolicy"; // Nombre de la política CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MiCors,
        policy =>
        {
            // Reemplaza con el puerto donde corre tu Angular si es necesario
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Muy importante para Sesiones/Cookies
        });
});

// Aprende más sobre la configuración de Swagger/OpenAPI en https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- FIN DE CONFIGURACIÓN DE SERVICIOS ---

var app = builder.Build();

// --- CONFIGURACIÓN DE PIPELINE DE PETICIONES HTTP ---

// Configure el pipeline de peticiones HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar la política CORS definida
app.UseCors(MiCors);

// Redireccionamiento HTTPS (se recomienda)
app.UseHttpsRedirection();

// Habilitar el uso de Sesiones (DEBE ir antes de UseAuthorization)
app.UseSession();

// No necesitamos UseAuthorization si usamos sesiones simples y APIs Key privadas.
// app.UseAuthorization(); 

app.MapControllers();

app.Run();