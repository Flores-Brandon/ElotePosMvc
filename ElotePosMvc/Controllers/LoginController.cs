using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity; // <--- Necesario para desencriptar
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        // Inyectamos la base de datos Y el encriptador
        public LoginController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
        {
            _coldDb = coldDbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            // 1. Buscamos al usuario SOLO por el nombre (sin checar password todavía)
            var usuario = await _coldDb.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == login.Username);

            if (usuario == null)
            {
                return Unauthorized("El usuario no existe.");
            }

            if (!usuario.Activo)
            {
                return Unauthorized("Este usuario está dado de baja.");
            }

            // 2. VERIFICAMOS LA CONTRASEÑA ENCRIPTADA
            // El hasher compara la pass que escribió Diego con la pass encriptada de la BD
            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, login.Password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                // Si falló la verificación, probamos si es una contraseña vieja (sin encriptar, como la del Admin)
                if (usuario.PasswordHash != login.Password)
                {
                    return Unauthorized("Contraseña incorrecta.");
                }
            }

            // 3. Si pasó, guardamos la sesión
            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "Empleado");

            // 4. Respondemos al Frontend
            return Ok(new
            {
                mensaje = "Login exitoso",
                usuario = usuario.Username,
                rol = usuario.Rol?.Nombre,
                idUsuario = usuario.IdUsuario
            });
        }
    }

    // Clase auxiliar para recibir los datos del post
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}