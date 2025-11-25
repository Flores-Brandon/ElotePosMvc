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

        public InsumosController(ColdDbContext coldDbContext)
        {
            _coldDb = coldDbContext;
        }

        private bool EsJefe()
        {
            return HttpContext.Session.GetString("Rol") == "Jefe";
        }

        // GET: api/insumos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Insumo>>> GetInsumos()
        {
            if (!EsJefe()) return Unauthorized("Solo el Jefe puede ver insumos");
            return await _coldDb.Insumos.ToListAsync();
        }

        // POST: api/insumos
        [HttpPost]
        public async Task<ActionResult<Insumo>> PostInsumo(Insumo insumo)
        {
            if (!EsJefe()) return Unauthorized("Solo el Jefe puede crear insumos");

            insumo.IdInsumo = 0; // Aseguramos que sea nuevo
            _coldDb.Insumos.Add(insumo);
            await _coldDb.SaveChangesAsync();

            return CreatedAtAction("GetInsumos", new { id = insumo.IdInsumo }, insumo);
        }

        // PUT: api/insumos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInsumo(int id, Insumo insumo)
        {
            if (!EsJefe()) return Unauthorized("Solo el Jefe puede editar");

            if (id != insumo.IdInsumo) return BadRequest();

            _coldDb.Entry(insumo).State = EntityState.Modified;
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/insumos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInsumo(int id)
        {
            if (!EsJefe()) return Unauthorized("Solo el Jefe puede borrar");

            var insumo = await _coldDb.Insumos.FindAsync(id);
            if (insumo == null) return NotFound();

            _coldDb.Insumos.Remove(insumo);
            await _coldDb.SaveChangesAsync();

            return NoContent();
        }
    }
}