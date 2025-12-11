using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly HotDbContext _hotDb;
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public TurnosController(HotDbContext hotDb, ColdDbContext coldDb, IPasswordHasher<Usuario> hasher)
        {
            _hotDb = hotDb;
            _coldDb = coldDb;
            _passwordHasher = hasher;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("IdUsuario") ?? 1;
        }

        // 1. VERIFICAR ESTADO
        [HttpGet("estado")]
        public async Task<IActionResult> VerificarEstado()
        {
            var ultimoTurno = await _hotDb.Turnos
                                         .OrderByDescending(t => t.IdTurno)
                                         .FirstOrDefaultAsync();

            if (ultimoTurno == null)
            {
                return Ok(new { abierto = false, usuarioCierra = "Sistema Nuevo" });
            }

            if (ultimoTurno.FechaCierre == null)
            {
                var nombreUsuario = "Desconocido";
                var usuario = await _coldDb.Usuarios.FindAsync(ultimoTurno.IdUsuarioAbre);
                if (usuario != null) nombreUsuario = usuario.NombreCompleto ?? usuario.Username;

                return Ok(new
                {
                    abierto = true,
                    turno = ultimoTurno,
                    usuarioAbre = nombreUsuario
                });
            }
            else
            {
                var nombreUsuario = "Desconocido";
                var usuario = await _coldDb.Usuarios.FindAsync(ultimoTurno.IdUsuarioCierra);
                if (usuario != null) nombreUsuario = usuario.NombreCompleto ?? usuario.Username;

                return Ok(new
                {
                    abierto = false,
                    usuarioCierra = nombreUsuario
                });
            }
        }

        private async Task<Usuario?> ValidarUsuario(string username, string password)
        {
            var usuario = await _coldDb.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
            if (usuario == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed) return null;

            return usuario;
        }

        // 2. ABRIR CAJA
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaDto dto)
        {
            if (await _hotDb.Turnos.AnyAsync(t => t.FechaCierre == null))
                return BadRequest("Ya hay una caja abierta.");

            var usuarioAutorizado = await ValidarUsuario(dto.Username, dto.Password);
            if (usuarioAutorizado == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            var fechaInicio = DateTime.Now;
            var fechaStr = fechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
            int idUsuario = usuarioAutorizado.IdUsuario;

            string sqlInsert = $@"
                EXEC('
                    INSERT INTO turnos (FechaInicio, SaldoInicial, IdUsuarioAbre, IdUsuarioCreacion)
                    VALUES (''{fechaStr}'', {dto.SaldoInicial}, {idUsuario}, {idUsuario})
                ') AT MYSQL_LINK";

            await _hotDb.Database.ExecuteSqlRawAsync(sqlInsert);

            var nuevoTurno = await _hotDb.Turnos.OrderByDescending(t => t.IdTurno).FirstOrDefaultAsync();
            return Ok(nuevoTurno);
        }

        // 3. CERRAR CAJA
        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaDto dto)
        {
            var turno = await _hotDb.Turnos
                                .OrderByDescending(t => t.IdTurno)
                                .FirstOrDefaultAsync(t => t.FechaCierre == null);

            if (turno == null) return BadRequest("No hay caja abierta.");

            var usuarioAutorizado = await ValidarUsuario(dto.Username, dto.Password);
            if (usuarioAutorizado == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            var fechaCierre = DateTime.Now;
            var fechaStr = fechaCierre.ToString("yyyy-MM-dd HH:mm:ss");
            int idUsuario = usuarioAutorizado.IdUsuario;

            string sqlUpdate = $@"
                EXEC('
                    UPDATE turnos 
                    SET FechaCierre = ''{fechaStr}'', 
                        IdUsuarioCierra = {idUsuario},
                        IdUsuarioModificacion = {idUsuario} 
                    WHERE IdTurno = {turno.IdTurno}
                ') AT MYSQL_LINK";

            await _hotDb.Database.ExecuteSqlRawAsync(sqlUpdate);

            return Ok(new { mensaje = "Caja cerrada correctamente" });
        }

        // 4. OBTENER RESUMEN (CORREGIDO CON IDs)
        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen()
        {
            try
            {
                var turno = await _hotDb.Turnos.OrderByDescending(t => t.IdTurno).FirstOrDefaultAsync();

                if (turno == null)
                    return Ok(new { saldoInicial = 0, ventasEfectivo = 0, ventasTarjeta = 0, ventasRegalado = 0, totalCajon = 0 });

                var ventasDelTurno = await _hotDb.Ventas
                                               .Where(v => v.IdTurno == turno.IdTurno)
                                               .ToListAsync();

                // ----------------------------------------------------
                // 🛠️ AQUÍ ESTÁ LA CORRECCIÓN DE LOS ERRORES CS1061
                // Usamos los IDs en lugar de los Strings/Booleanos viejos
                // ----------------------------------------------------

                // ID 1 = Efectivo
                decimal efectivo = ventasDelTurno.Where(v => v.IdFormaPago == 1).Sum(v => v.TotalVenta);

                // ID 2 = Tarjeta (o cualquier cosa que no sea efectivo)
                decimal tarjeta = ventasDelTurno.Where(v => v.IdFormaPago != 1).Sum(v => v.TotalVenta);

                // ID 2 = Tipo Venta "Cortesía/Regalo"
                decimal regalo = ventasDelTurno.Where(v => v.IdTipoVenta == 2).Sum(v => v.TotalVenta);

                decimal totalEnCajon = turno.SaldoInicial + efectivo;

                return Ok(new
                {
                    turnoId = turno.IdTurno,
                    saldoInicial = turno.SaldoInicial,
                    ventasEfectivo = efectivo,
                    ventasTarjeta = tarjeta,
                    ventasRegalado = regalo,
                    totalCajon = totalEnCajon
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Error obteniendo resumen: " + ex.Message);
            }
        }
    }
}