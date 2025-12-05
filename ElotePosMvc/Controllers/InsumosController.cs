using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsumosController : ControllerBase
    {
        private readonly ColdDbContext _coldDb;

        public InsumosController(ColdDbContext coldDb)
        {
            _coldDb = coldDb;
        }

        // GET: api/insumos
        [HttpGet]
        public async Task<IActionResult> GetInsumos()
        {
            try
            {
                // Intentamos traer los datos
                var lista = await _coldDb.Insumos.ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                // 🛑 AQUÍ ESTÁ EL TRUCO:
                // Si falla, devolvemos el error exacto (incluyendo el error interno)
                return BadRequest(new
                {
                    mensaje = "Error al leer Insumos",
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        // POST: api/insumos (Crear)
        [HttpPost]
        public async Task<ActionResult<Insumo>> PostInsumo(Insumo insumo)
        {
            _coldDb.Insumos.Add(insumo);
            await _coldDb.SaveChangesAsync();
            return CreatedAtAction("GetInsumos", new { id = insumo.IdInsumo }, insumo);
        }

        // PUT: api/insumos/5 (Editar)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInsumo(int id, Insumo insumo)
        {
            if (id != insumo.IdInsumo) return BadRequest();

            _coldDb.Entry(insumo).State = EntityState.Modified;
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/insumos/5 (Borrar)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInsumo(int id)
        {
            var insumo = await _coldDb.Insumos.FindAsync(id);
            if (insumo == null) return NotFound();

            _coldDb.Insumos.Remove(insumo);
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }
    }
}