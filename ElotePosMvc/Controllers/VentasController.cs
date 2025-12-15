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

        // =========================================================================
        // GET: api/ventas
        // ✅ APROBADO POR EL MAESTRO: Usa Stored Procedure con Pass-Through
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetVentas(
            [FromQuery] DateTime? inicio,
            [FromQuery] DateTime? fin,
            [FromQuery] bool exportar = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int cantidad = 10)
        {
            try
            {
                // Validación de fechas
                if (!inicio.HasValue) inicio = DateTime.Today;
                if (!fin.HasValue) fin = DateTime.Today.AddDays(1).AddSeconds(-1);
                else fin = fin.Value.Date.AddDays(1).AddSeconds(-1);

                // Formateamos fechas para SQL
                string fechaIniStr = inicio.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string fechaFinStr = fin.Value.ToString("yyyy-MM-dd HH:mm:ss");

                // 1. OBTENER EL TOTAL (Para que Angular sepa cuántas páginas son)
                // Usamos OPENQUERY para contar rápido sin traer los datos
                int totalRegistros = 0;

                if (!exportar)
                {
                    string sqlCount = $@"
                        SELECT Total FROM OPENQUERY(MYSQL_LINK, 
                        'SELECT COUNT(*) as Total FROM ventas 
                         WHERE FechaHora >= ''{fechaIniStr}'' AND FechaHora <= ''{fechaFinStr}'' ')";

                    try
                    {
                        using (var command = _hotDb.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = sqlCount;
                            _hotDb.Database.OpenConnection();
                            using (var result = await command.ExecuteReaderAsync())
                            {
                                if (result.Read()) totalRegistros = Convert.ToInt32(result["Total"]);
                            }
                        }
                    }
                    catch { totalRegistros = 1000; } // Fallback si falla el conteo
                }

                // 2. TRAER LOS DATOS USANDO EL STORED PROCEDURE
                // Esto ejecuta la consulta directamente en MySQL (Pass-Through)
                // C# le manda: EXEC sp_ObtenerHistorialPassThrough ...
                var listaDatos = await _hotDb.Ventas
                    .FromSqlRaw("EXEC sp_ObtenerHistorialPassThrough {0}, {1}, {2}, {3}, {4}",
                                inicio, fin, pagina, cantidad, exportar)
                    .ToListAsync();

                // 3. RETORNAR JSON
                return Ok(new
                {
                    Total = exportar ? listaDatos.Count : totalRegistros,
                    Datos = listaDatos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en Pass-Through", error = ex.Message });
            }
        }

        // GET: api/ventas/hoy (Dashboard)
        [HttpGet("hoy")]
        public async Task<IActionResult> ObtenerVentasHoy()
        {
            try
            {
                // Aquí usamos AsNoTracking para que sea ligero
                var ultimasVentas = await _hotDb.Ventas
                                    .AsNoTracking()
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

        // POST: api/ventas (Cobrar)
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

            // FORMATO DECIMAL SEGURO
            string totalStr = venta.TotalVenta.ToString(CultureInfo.InvariantCulture);
            string pagoStr = venta.PagoRecibido.ToString(CultureInfo.InvariantCulture);
            string cambioStr = venta.CambioDado.ToString(CultureInfo.InvariantCulture);
            string fechaFormat = venta.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");

            // SQL RAW PASS-THROUGH (INSERT)
            string sqlInsertVenta = $@"
                EXEC('
                    INSERT INTO ventas (IdTurno, IdUsuario, TotalVenta, PagoRecibido, CambioDado, IdFormaPago, IdTipoVenta, FechaHora, IdUsuarioCreacion)
                    VALUES ({venta.IdTurno}, {venta.IdUsuario}, {totalStr}, {pagoStr}, {cambioStr}, {venta.IdFormaPago}, {venta.IdTipoVenta}, ''{fechaFormat}'', {idUsuarioLogueado})
                ') AT MYSQL_LINK";

            try
            {
                await _hotDb.Database.ExecuteSqlRawAsync(sqlInsertVenta);

                // RECUPERAR ID (Consultamos la tabla local mapeada para obtener el último ID)
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
                return StatusCode(500, "Error guardando venta: " + ex.Message);
            }
        }
    }
}