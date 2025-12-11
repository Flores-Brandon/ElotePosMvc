using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;
        private const string API_KEY_SECRETA = "EloteSecreto2025";

        public ProductosController(ColdDbContext coldDbContext)
        {
            _coldDb = coldDbContext;
        }

        // --- SEGURIDAD ---
        private bool EsLaApiAdmin() => Request.Headers["X-ELOTE-KEY"].FirstOrDefault() == API_KEY_SECRETA;

        private bool TienePermisoLectura()
        {
            if (EsLaApiAdmin()) return true;
            return HttpContext.Session.GetInt32("IdUsuario") != null;
        }

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
            if (!TienePermisoLectura()) return Unauthorized("Acceso denegado.");

            // ⚠️ FILTRO IMPORTANTE: Solo traemos los Activos
            return await _coldDb.Productos
                                .Where(p => p.Activo == true)
                                .ToListAsync();
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            if (!TienePermisoLectura()) return Unauthorized("Acceso denegado.");

            var producto = await _coldDb.Productos.FindAsync(id);

            // Verificamos que exista Y que esté activo
            if (producto == null || producto.Activo == false) return NotFound();

            return producto;
        }

        // POST: api/productos (Crear + Auditoría)
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");

            // 1. Obtener Usuario Actual
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            // 2. Llenar Auditoría Automática
            producto.IdProducto = 0; // Nuevo ID
            producto.Activo = true;  // Nace vivo
            producto.IdUsuarioCreacion = idUsuario ?? 1; // 1 si no hay sesión (Admin por defecto)
            producto.FechaCreacion = DateTime.Now;

            // Limpiamos modificación por si acaso
            producto.IdUsuarioModificacion = null;
            producto.FechaModificacion = null;

            _coldDb.Productos.Add(producto);
            await _coldDb.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = producto.IdProducto }, producto);
        }

        // PUT: api/productos/5 (Editar + Auditoría)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto productoFront)
        {
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");
            if (id != productoFront.IdProducto) return BadRequest("IDs no coinciden");

            // 1. Buscamos el original en BD para no perder datos de creación
            var productoDB = await _coldDb.Productos.FindAsync(id);
            if (productoDB == null || productoDB.Activo == false) return NotFound();

            // 2. Obtener Usuario Actual
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            // 3. Actualizar DATOS DE NEGOCIO
            productoDB.Nombre = productoFront.Nombre;
            productoDB.PrecioVenta = productoFront.PrecioVenta;
            // (Si tienes IdCategoria u otros, actualízalos aquí también)

            // 4. Actualizar AUDITORÍA (Quién modificó)
            productoDB.IdUsuarioModificacion = idUsuario ?? 1;
            productoDB.FechaModificacion = DateTime.Now;

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

        // DELETE: api/productos/5 (Soft Delete + Auditoría)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            if (!TienePermisoEscritura()) return Unauthorized("Se requiere permiso de Jefe.");

            var producto = await _coldDb.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            // 1. Obtener Usuario Actual
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            // 2. BORRADO LÓGICO (No Removemos, solo desactivamos)
            producto.Activo = false;

            // 3. Registramos quién lo eliminó
            producto.IdUsuarioModificacion = idUsuario ?? 1;
            producto.FechaModificacion = DateTime.Now;

            await _coldDb.SaveChangesAsync();

            return NoContent();
        }
    }
}