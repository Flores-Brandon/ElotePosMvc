using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElotePosMvc.Controllers
{
    // 1. Definimos que esto es una API y su ruta será 'api/productos'
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase // 2. Heredamos de ControllerBase (ideal para APIs)
    {
        private readonly ColdDbContext _coldDb;

        public ProductosController(ColdDbContext coldDbContext)
        {
            _coldDb = coldDbContext;
        }

        // --- MÉTODOS AUXILIARES DE VALIDACIÓN ---

        // Valida si es el Patrón (Para crear, editar, borrar)
        private bool EsJefe()
        {
            return HttpContext.Session.GetString("Rol") == "Jefe";
        }

        // 👇 NUEVO: Valida si es CUALQUIER usuario logueado (Para ver el menú)
        private bool HaySesion()
        {
            // Si tiene ID de usuario en la sesión, es que ya entró al sistema
            return HttpContext.Session.GetInt32("IdUsuario") != null;
        }

        // GET: api/productos
        // (Obtener todos los productos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            // 🔓 CAMBIO IMPORTANTE: 
            // Antes usábamos !EsJefe(), lo que bloqueaba a los empleados.
            // Ahora usamos !HaySesion(), permitiendo que CUALQUIERA que haya hecho login vea la lista.
            if (!HaySesion()) return Unauthorized("Debes iniciar sesión para ver el menú");

            return await _coldDb.Productos.ToListAsync();
        }

        // GET: api/productos/5
        // (Obtener un solo producto por ID - Usualmente para editar)
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            // Este lo dejamos protegido porque usualmente solo pides un ID específico para editarlo
            if (!EsJefe()) return Unauthorized("No tienes permiso de Jefe");

            var producto = await _coldDb.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }

        // POST: api/productos
        // (Crear un nuevo producto)
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // 🔒 Protegido: Solo Jefe
            if (!EsJefe()) return Unauthorized("No tienes permiso de Jefe");

            producto.IdProducto = 0;

            _coldDb.Productos.Add(producto);
            await _coldDb.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = producto.IdProducto }, producto);
        }

        // PUT: api/productos/5
        // (Actualizar un producto existente)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            // 🔒 Protegido: Solo Jefe
            if (!EsJefe()) return Unauthorized("No tienes permiso de Jefe");

            if (id != producto.IdProducto)
            {
                return BadRequest("El ID de la URL no coincide con el del producto");
            }

            _coldDb.Entry(producto).State = EntityState.Modified;

            try
            {
                await _coldDb.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_coldDb.Productos.Any(e => e.IdProducto == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/productos/5
        // (Eliminar un producto)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            // 🔒 Protegido: Solo Jefe
            if (!EsJefe()) return Unauthorized("No tienes permiso de Jefe");

            var producto = await _coldDb.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _coldDb.Productos.Remove(producto);
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }
    }
}