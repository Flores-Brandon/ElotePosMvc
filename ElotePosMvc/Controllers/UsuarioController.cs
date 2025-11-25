using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuariosController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
        {
            _coldDb = coldDbContext;
            _passwordHasher = passwordHasher;
        }

        // GET: api/usuarios (Listar empleados)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetUsuarios()
        {
            // Solo devolvemos lo necesario (sin la contraseña encriptada)
            return await _coldDb.Usuarios
                .Include(u => u.Rol)
                .Select(u => new {
                    u.IdUsuario,
                    u.NombreCompleto,
                    u.Username,
                    Rol = u.Rol.Nombre,
                    u.Activo
                })
                .ToListAsync();
        }

        // POST: api/usuarios (Crear nuevo)
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // 1. Validar si el usuario ya existe
            if (await _coldDb.Usuarios.AnyAsync(u => u.Username == usuario.Username))
            {
                return BadRequest("Ese nombre de usuario ya está ocupado.");
            }

            // 2. ENCRIPTAR LA CONTRASEÑA (Seguridad)
            // Tomamos la contraseña que viene plana y la convertimos en hash
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, usuario.PasswordHash);

            // 3. Configuración por defecto
            usuario.Activo = true;

            // 4. Guardar
            _coldDb.Usuarios.Add(usuario);
            await _coldDb.SaveChangesAsync();

            return Ok(new { mensaje = "Empleado creado con éxito" });
        }
    }
}