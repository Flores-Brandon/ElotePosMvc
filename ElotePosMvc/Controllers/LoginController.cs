using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity; // Necesario para IPasswordHasher

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        // Inyectamos el contexto (que ahora apunta a SQL Server) y el hasher
        public LoginController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
        {
            _coldDb = coldDbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                // 1. Buscamos al usuario
                var usuario = await _coldDb.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Username == login.Username);

                if (usuario == null) return Unauthorized(new { message = "El usuario no existe." });
                if (!usuario.Activo) return Unauthorized(new { message = "Usuario inactivo." });

                // 2. Verificamos password
                var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, login.Password);

                if (resultado == PasswordVerificationResult.Failed)
                {
                    if (usuario.PasswordHash != login.Password) return Unauthorized(new { message = "Contraseña incorrecta." });
                }

                // 3. INTENTO DE CREAR SESIÓN (Aquí suele fallar si falta configuración)
                HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
                HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto ?? usuario.Username);
                HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "Empleado");

                await HttpContext.Session.CommitAsync();

                return Ok(new
                {
                    message = "Login exitoso",
                    usuario = usuario.Username,
                    rol = usuario.Rol?.Nombre,
                    idUsuario = usuario.IdUsuario
                });
            }
            catch (Exception ex)
            {
                // ESTO NOS DIRÁ EL ERROR EXACTO EN LA CONSOLA DEL NAVEGADOR
                return StatusCode(500, new
                {
                    error = "Error Interno",
                    detalle = ex.Message,
                    origen = ex.StackTrace
                });
            }
        }
        // Endpoint extra para cerrar sesión
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Sesión cerrada correctamente" });
        }
    }

    // DTO para recibir los datos
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}