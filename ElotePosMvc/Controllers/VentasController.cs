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

        // 👇 1. FUNCIÓN AUXILIAR: Obtener Hora de México (Centro)
        // Esto evita que las ventas se guarden con hora del futuro (UTC)
        private DateTime ObtenerHoraMexico()
        {
            try
            {
                // Intentamos obtener la zona horaria de Windows
                var zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaMexico);
            }
            catch
            {
                try
                {
                    // Si falla (Linux/Docker), intentamos el ID estándar IANA
                    var zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaMexico);
                }
                catch
                {
                    // Si todo falla, restamos 6 horas manualmente como plan de emergencia
                    return DateTime.UtcNow.AddHours(-6);
                }
            }
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            // A. BUSCAR EL TURNO ABIERTO
            var turnoAbierto = await _hotDb.Turnos
                                        .OrderByDescending(t => t.IdTurno)
                                        .FirstOrDefaultAsync(t => t.FechaCierre == null);

            // B. VALIDAR SI LA CAJA ESTÁ CERRADA
            if (turnoAbierto == null)
            {
                return BadRequest("❌ LA CAJA ESTÁ CERRADA. No puedes vender hasta abrir turno.");
            }

            // C. Asignar datos automáticos
            venta.IdTurno = turnoAbierto.IdTurno;

            // 👇 AQUÍ ESTÁ EL ARREGLO DE LA HORA
            venta.FechaHora = ObtenerHoraMexico();

            // D. Asignar Usuario (Mantenemos tu lógica actual)
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            venta.IdUsuario = idUsuarioLogueado ?? 1;

            // E. Calcular cambio si hace falta
            if (venta.MetodoPago == "Efectivo" && venta.CambioDado == 0 && venta.PagoRecibido >= venta.TotalVenta)
            {
                venta.CambioDado = venta.PagoRecibido - venta.TotalVenta;
            }

            // F. Guardar
            _hotDb.Ventas.Add(venta);
            await _hotDb.SaveChangesAsync();

            return Ok(venta);
        }

        // GET: api/ventas/hoy 
        // 👇 ESTE ES EL NUEVO MÉTODO PARA TU PANTALLA DE HISTORIAL DIARIO
        [HttpGet("hoy")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasHoy()
        {
            // Calculamos "Hoy a las 00:00" en hora de México
            var hoyEnMexico = ObtenerHoraMexico().Date;

            // Filtramos ventas desde hoy a las 00:00 hasta mañana a las 00:00
            var ventas = await _hotDb.Ventas
                .Where(v => v.FechaHora >= hoyEnMexico && v.FechaHora < hoyEnMexico.AddDays(1))
                .OrderByDescending(v => v.FechaHora) // Las más recientes primero
                .ToListAsync();

            return Ok(ventas);
        }

        // GET: api/ventas (FILTRO GENERAL POR FECHAS)
        // Este lo dejamos igual, sirve para reportes de rangos específicos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fin)
        {
            var query = _hotDb.Ventas.AsQueryable();

            if (inicio.HasValue)
            {
                query = query.Where(v => v.FechaHora >= inicio.Value);
            }

            if (fin.HasValue)
            {
                var finDia = fin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(v => v.FechaHora <= finDia);
            }

            return await query.OrderByDescending(v => v.FechaHora).ToListAsync();
        }
    }
}