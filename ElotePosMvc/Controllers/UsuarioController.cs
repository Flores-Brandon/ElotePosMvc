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

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetUsuarios()
        {
            return await _coldDb.Usuarios
                .Where(u => u.Activo == true) // 👈 FILTRO CLAVE: Solo traemos los vivos
                .Include(u => u.Rol)
                .Select(u => new {
                    u.IdUsuario,
                    u.NombreCompleto,
                    u.Username,
                    IdRol = u.IdRol, // Necesario para editar
                    Rol = u.Rol.Nombre,
                    u.Activo
                })
                .ToListAsync();
        }

        // PUT: api/usuarios/5 (Editar)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            var usuarioDb = await _coldDb.Usuarios.FindAsync(id);
            if (usuarioDb == null) return NotFound();

            // Actualizamos datos básicos
            usuarioDb.NombreCompleto = usuario.NombreCompleto;
            usuarioDb.Username = usuario.Username;
            usuarioDb.IdRol = usuario.IdRol;

            // Lógica inteligente para la contraseña:
            // Si el campo PasswordHash viene con texto, la actualizamos.
            // Si viene vacío o null, dejamos la contraseña vieja intacta.
            if (!string.IsNullOrEmpty(usuario.PasswordHash))
            {
                usuarioDb.PasswordHash = _passwordHasher.HashPassword(usuarioDb, usuario.PasswordHash);
            }

            await _coldDb.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/usuarios/5 (Baja Lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _coldDb.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // NO BORRAMOS, SOLO DESACTIVAMOS
            usuario.Activo = false;

            await _coldDb.SaveChangesAsync();
            return NoContent();
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