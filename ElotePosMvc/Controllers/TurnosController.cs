using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly HotDbContext _hotDb;   // Para guardar el Turno (MySQL)
        private readonly ColdDbContext _coldDb; // Para verificar el Usuario (SQL Server)
        private readonly IPasswordHasher<Usuario> _passwordHasher; // Para checar la contraseña

        // Inyectamos los 3 servicios
        public TurnosController(HotDbContext hotDb, ColdDbContext coldDb, IPasswordHasher<Usuario> hasher)
        {
            _hotDb = hotDb;
            _coldDb = coldDb;
            _passwordHasher = hasher;
        }

        [HttpGet("estado")]
        public async Task<IActionResult> VerificarEstado()
        {
            var turnoAbierto = await _hotDb.Turnos
                                    .OrderByDescending(t => t.IdTurno)
                                    .FirstOrDefaultAsync(t => t.FechaCierre == null);

            if (turnoAbierto != null) return Ok(new { abierto = true, turno = turnoAbierto });
            return Ok(new { abierto = false });
        }

        // --- MÉTODO AUXILIAR PARA VALIDAR CREDENCIALES ---
        private async Task<Usuario?> ValidarUsuario(string username, string password)
        {
            var usuario = await _coldDb.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
            if (usuario == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed) return null;

            return usuario;
        }

        // 2. ABRIR CAJA (Con contraseña)
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaDto dto)
        {
            // A. Verificar si ya hay caja abierta
            if (await _hotDb.Turnos.AnyAsync(t => t.FechaCierre == null))
                return BadRequest("Ya hay una caja abierta.");

            // B. Validar Usuario y Contraseña
            var usuarioAutorizado = await ValidarUsuario(dto.Username, dto.Password);
            if (usuarioAutorizado == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            // C. Crear Turno
            var nuevoTurno = new Turno
            {
                FechaInicio = DateTime.Now,
                SaldoInicial = dto.SaldoInicial,
                IdUsuarioAbre = usuarioAutorizado.IdUsuario // Usamos el ID del que puso la contraseña
            };

            _hotDb.Turnos.Add(nuevoTurno);
            await _hotDb.SaveChangesAsync();

            return Ok(nuevoTurno);
        }

        // 3. CERRAR CAJA (Con contraseña)
        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaDto dto)
        {
            // A. Buscar turno abierto
            var turno = await _hotDb.Turnos
                            .OrderByDescending(t => t.IdTurno)
                            .FirstOrDefaultAsync(t => t.FechaCierre == null);

            if (turno == null) return BadRequest("No hay caja abierta.");

            // B. Validar Usuario y Contraseña
            var usuarioAutorizado = await ValidarUsuario(dto.Username, dto.Password);
            if (usuarioAutorizado == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            // C. Cerrar Turno
            turno.FechaCierre = DateTime.Now;
            turno.IdUsuarioCierra = usuarioAutorizado.IdUsuario; // Quién cerró realmente

            await _hotDb.SaveChangesAsync();
            return Ok(new { mensaje = "Caja cerrada correctamente" });
        }

        // 4. OBTENER RESUMEN (CORTE DE CAJA)
        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen()
        {
            // A. Buscar el turno abierto
            var turno = await _hotDb.Turnos
                            .OrderByDescending(t => t.IdTurno)
                            .FirstOrDefaultAsync(t => t.FechaCierre == null);

            if (turno == null) return NotFound("No hay turno abierto.");

            // B. Traer todas las ventas de este turno
            var ventas = await _hotDb.Ventas
                            .Where(v => v.IdTurno == turno.IdTurno)
                            .ToListAsync();

            // C. Calcular Totales
            var resumen = new ResumenTurnoDto
            {
                IdTurno = turno.IdTurno,
                FechaInicio = turno.FechaInicio,
                SaldoInicial = turno.SaldoInicial,

                // Sumamos usando LINQ según el método de pago
                TotalEfectivo = ventas.Where(v => v.MetodoPago == "Efectivo").Sum(v => v.TotalVenta),
                TotalTarjeta = ventas.Where(v => v.MetodoPago == "Tarjeta").Sum(v => v.TotalVenta),
                TotalRegalos = ventas.Where(v => v.MetodoPago == "Regalo").Sum(v => v.TotalVenta) // Usamos TotalVenta (precio real) aunque haya sido gratis, para saber la merma
            };

            // D. Calcular finales
            resumen.TotalVendido = resumen.TotalEfectivo + resumen.TotalTarjeta; // Regalos no suman a la venta monetaria
            resumen.DineroEnCaja = resumen.SaldoInicial + resumen.TotalEfectivo; // Lo de tarjeta está en el banco

            return Ok(resumen);
        }
    }
}