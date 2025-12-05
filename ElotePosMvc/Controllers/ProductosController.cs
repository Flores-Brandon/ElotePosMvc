using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // Necesario para Headers
using System.Threading.Tasks;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;

        // 🔐 TU CONTRASEÑA MAESTRA
        // Debe ser IGUAL a la que pusiste en el MVC
        private const string API_KEY_SECRETA = "EloteSecreto2025";

        public ProductosController(ColdDbContext coldDbContext)
        {
            _coldDb = coldDbContext;
        }

        // --- MÉTODOS DE SEGURIDAD ---

        // Validar si la petición trae la Llave Maestra (Desde tu MVC)
        private bool EsLaApiAdmin()
        {
            var llaveRecibida = Request.Headers["X-ELOTE-KEY"].FirstOrDefault();
            return llaveRecibida == API_KEY_SECRETA;
        }

        // Validar Permiso de LECTURA (Ver menú)
        // Pasa si: Es la API Admin -O- Hay cualquier usuario logueado en caja
        private bool TienePermisoLectura()
        {
            if (EsLaApiAdmin()) return true;
            return HttpContext.Session.GetInt32("IdUsuario") != null;
        }

        // Validar Permiso de ESCRITURA (Crear/Borrar)
        // Pasa si: Es la API Admin -O- El usuario en caja es "Jefe"
        private bool TienePermisoEscritura()
        {
            if (EsLaApiAdmin()) return true;
            return HttpContext.Session.GetString("Rol") == "Jefe";
        }

        // --- ENDPOINTS ---

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            // Verificamos lectura (Cualquier empleado o el MVC)
            if (!TienePermisoLectura()) return Unauthorized("Acceso denegado.");

            return await _coldDb.Productos.ToListAsync();
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            // Para ver detalle también permitimos lectura general
            if (!TienePermisoLectura()) return Unauthorized("Acceso denegado.");

            var producto = await _coldDb.Productos.FindAsync(id);

            if (producto == null) return NotFound();

            return producto;
        }

        // POST: api/productos (Crear)
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // 🔒 Solo Jefe o API Admin pueden crear
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");

            producto.IdProducto = 0; // Aseguramos que sea nuevo ID
            _coldDb.Productos.Add(producto);
            await _coldDb.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = producto.IdProducto }, producto);
        }

        // PUT: api/productos/5 (Editar)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            // 🔒 Solo Jefe o API Admin pueden editar
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");

            if (id != producto.IdProducto) return BadRequest("IDs no coinciden");

            _coldDb.Entry(producto).State = EntityState.Modified;

            try
            {
                await _coldDb.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_coldDb.Productos.Any(e => e.IdProducto == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/productos/5 (Borrar)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            // 🔒 Solo Jefe o API Admin pueden borrar
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");

            var producto = await _coldDb.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            _coldDb.Productos.Remove(producto);
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }
    }
}