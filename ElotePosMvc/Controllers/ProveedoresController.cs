using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly HotDbContext _db;

        public ProveedoresController(HotDbContext context)
        {
            _db = context;
        }

        // GET: api/proveedores
        [HttpGet]
        public async Task<IActionResult> GetProveedores()
        {
            // Solo traemos los activos
            var lista = await _db.Proveedores.Where(p => p.Activo == true).ToListAsync();
            return Ok(lista);
        }

        // POST: api/proveedores (Crear)
        [HttpPost]
        public async Task<IActionResult> Crear(Proveedor proveedor)
        {
            proveedor.Activo = true; // Por defecto nace activo
            _db.Proveedores.Add(proveedor);
            await _db.SaveChangesAsync();
            return Ok(proveedor);
        }

        // PUT: api/proveedores/5 (Editar)
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor) return BadRequest();

            var existente = await _db.Proveedores.FindAsync(id);
            if (existente == null) return NotFound();

            // Actualizamos campos
            existente.Empresa = proveedor.Empresa;
            existente.Contacto = proveedor.Contacto;
            existente.Telefono = proveedor.Telefono;

            await _db.SaveChangesAsync();
            return Ok(existente);
        }

        // DELETE: api/proveedores/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var existente = await _db.Proveedores.FindAsync(id);
            if (existente == null) return NotFound();

            // ⚠️ NO BORRAMOS, SOLO DESACTIVAMOS
            existente.Activo = false;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Eliminado correctamente" });
        }
    }
}