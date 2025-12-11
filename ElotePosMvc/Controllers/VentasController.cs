using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Globalization;

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

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("IdUsuario") ?? 1;
        }

        private DateTime ObtenerHoraMexico()
        {
            try
            {
                var zona = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
            }
            catch { return DateTime.UtcNow.AddHours(-6); }
        }

        // 👇 ESTE ES EL MÉTODO QUE TE FALTABA (Solución Error 405) 👇
        // GET: api/ventas?inicio=...&fin=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fin)
        {
            try
            {
                // Traemos los datos. EF usará la Vista de SQL Server que apunta a MySQL
                var queryBase = await _hotDb.Ventas
                                            .OrderByDescending(v => v.FechaHora)
                                            .Take(1000) // Límite de seguridad
                                            .ToListAsync();

                IEnumerable<Venta> resultado = queryBase;

                // Filtros de fecha en memoria (C#) para evitar problemas de traducción SQL
                if (inicio.HasValue)
                    resultado = resultado.Where(v => v.FechaHora.Date >= inicio.Value.Date);

                if (fin.HasValue)
                    resultado = resultado.Where(v => v.FechaHora.Date <= fin.Value.Date);

                return Ok(resultado.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error cargando historial", error = ex.Message });
            }
        }

        // GET: api/ventas/hoy (Para el Dashboard/Caja)
        [HttpGet("hoy")]
        public async Task<IActionResult> ObtenerVentasHoy()
        {
            try
            {
                var ultimasVentas = await _hotDb.Ventas
                                    .OrderByDescending(v => v.FechaHora)
                                    .Take(500)
                                    .ToListAsync();

                var hoy = DateTime.Today;
                var ventasDeHoy = ultimasVentas.Where(v => v.FechaHora.Date == hoy).ToList();
                var total = ventasDeHoy.Any() ? ventasDeHoy.Sum(v => v.TotalVenta) : 0;

                return Ok(total);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error calculando total hoy: " + ex.Message);
            }
        }

        // POST: api/ventas (Para cobrar)
        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            // A. VALIDACIONES
            var turnoAbierto = await _hotDb.Turnos.OrderByDescending(t => t.IdTurno).FirstOrDefaultAsync(t => t.FechaCierre == null);
            if (turnoAbierto == null) return BadRequest("❌ LA CAJA ESTÁ CERRADA.");

            // B. DATOS
            venta.IdTurno = turnoAbierto.IdTurno;
            venta.FechaHora = ObtenerHoraMexico();
            int idUsuarioLogueado = GetCurrentUserId();
            venta.IdUsuario = idUsuarioLogueado;

            if (venta.IdFormaPago == 1 && venta.CambioDado == 0 && venta.PagoRecibido >= venta.TotalVenta)
                venta.CambioDado = venta.PagoRecibido - venta.TotalVenta;

            // FORMATO DECIMAL SEGURO (Solución Error 500 al guardar)
            string totalStr = venta.TotalVenta.ToString(CultureInfo.InvariantCulture);
            string pagoStr = venta.PagoRecibido.ToString(CultureInfo.InvariantCulture);
            string cambioStr = venta.CambioDado.ToString(CultureInfo.InvariantCulture);
            string fechaFormat = venta.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");

            // SQL RAW VENTA
            string sqlInsertVenta = $@"
                EXEC('
                    INSERT INTO ventas (IdTurno, IdUsuario, TotalVenta, PagoRecibido, CambioDado, IdFormaPago, IdTipoVenta, FechaHora, IdUsuarioCreacion)
                    VALUES ({venta.IdTurno}, {venta.IdUsuario}, {totalStr}, {pagoStr}, {cambioStr}, {venta.IdFormaPago}, {venta.IdTipoVenta}, ''{fechaFormat}'', {idUsuarioLogueado})
                ') AT MYSQL_LINK";

            try
            {
                await _hotDb.Database.ExecuteSqlRawAsync(sqlInsertVenta);

                // RECUPERAR ID
                var ventaRecienCreada = await _hotDb.Ventas.OrderByDescending(v => v.IdVenta).FirstOrDefaultAsync();

                if (ventaRecienCreada != null && venta.Productos != null && venta.Productos.Any())
                {
                    int idVentaGenerado = ventaRecienCreada.IdVenta;

                    foreach (var prod in venta.Productos)
                    {
                        decimal subtotal = prod.Cantidad * prod.Precio;
                        string nombreLimpio = prod.Nombre.Replace("'", "");

                        string precioStr = prod.Precio.ToString(CultureInfo.InvariantCulture);
                        string subtotalStr = subtotal.ToString(CultureInfo.InvariantCulture);

                        // 🛠️ CORRECCIÓN PLURAL: Apuntamos a 'detalleventas'
                        string sqlInsertDetalle = $@"
                            EXEC('
                                INSERT INTO detalleventas (IdVenta, IdProducto, NombreProducto, Cantidad, PrecioUnitario, Subtotal, IdUsuarioCreacion)
                                VALUES ({idVentaGenerado}, {prod.IdProducto}, ''{nombreLimpio}'', {prod.Cantidad}, {precioStr}, {subtotalStr}, {idUsuarioLogueado})
                            ') AT MYSQL_LINK";

                        await _hotDb.Database.ExecuteSqlRawAsync(sqlInsertDetalle);
                    }
                }

                return Ok(venta);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR MYSQL: " + ex.Message);
                if (ex.InnerException != null) Console.WriteLine("INNER: " + ex.InnerException.Message);

                return StatusCode(500, "Error guardando venta: " + ex.Message);
            }
        }
    }
}