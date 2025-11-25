using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly HotDbContext _hotDb;

        public VentasController(HotDbContext hotDbContext)
        {
            _hotDb = hotDbContext;
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            // 1. BUSCAR EL TURNO ABIERTO (Esto arregla el corte de caja)
            var turnoAbierto = await _hotDb.Turnos
                                    .OrderByDescending(t => t.IdTurno)
                                    .FirstOrDefaultAsync(t => t.FechaCierre == null);

            // 2. VALIDAR SI LA CAJA ESTÁ CERRADA (Esto arregla la seguridad)
            if (turnoAbierto == null)
            {
                return BadRequest("❌ LA CAJA ESTÁ CERRADA. No puedes vender hasta abrir turno.");
            }

            // 3. Asignar datos automáticos
            venta.IdTurno = turnoAbierto.IdTurno; // <--- AQUÍ VINCULAMOS LA VENTA AL TURNO ACTUAL
            venta.FechaHora = DateTime.Now;

            // 4. Asignar Usuario
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            venta.IdUsuario = idUsuarioLogueado ?? 1;

            // 5. Calcular cambio si hace falta
            if (venta.MetodoPago == "Efectivo" && venta.CambioDado == 0 && venta.PagoRecibido >= venta.TotalVenta)
            {
                venta.CambioDado = venta.PagoRecibido - venta.TotalVenta;
            }

            // 6. Guardar
            _hotDb.Ventas.Add(venta);
            await _hotDb.SaveChangesAsync();

            return Ok(venta);
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            // Ordenamos por la más reciente
            return await _hotDb.Ventas.OrderByDescending(v => v.FechaHora).ToListAsync();
        }
    }
}