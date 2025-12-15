using ElotePosMvc.Data;
using ElotePosMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalController : ControllerBase
    {
        private readonly HotDbContext _db;

        public PersonalController(HotDbContext context)
        {
            _db = context;
        }

        // GET: api/personal
        [HttpGet]
        public async Task<IActionResult> GetPersonal()
        {
            var lista = await _db.Empleados
                .Include(e => e.Puesto)
                .ThenInclude(p => p.Departamento)
                .Where(e => e.Activo == true)
                .Select(e => new
                {
                    e.IdEmpleado,
                    e.Nombre,
                    e.Apellido,
                    NombreCompleto = e.Nombre + " " + e.Apellido,
                    e.IdPuesto, // Importante para el editar
                    Puesto = e.Puesto.Nombre,
                    Departamento = e.Puesto.Departamento.Nombre,
                    Salario = e.Puesto.SalarioBase,
                    FechaIngreso = e.FechaContratacion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/personal/puestos (Para llenar el select)
        [HttpGet("puestos")]
        public async Task<IActionResult> GetPuestos()
        {
            var lista = await _db.Puestos
                .Include(p => p.Departamento)
                .Select(p => new { p.IdPuesto, p.Nombre, Depto = p.Departamento.Nombre })
                .ToListAsync();
            return Ok(lista);
        }

        // POST: api/personal
        [HttpPost]
        public async Task<IActionResult> Crear(Empleado empleado)
        {
            empleado.Activo = true;
            empleado.FechaContratacion = DateTime.Now;
            _db.Empleados.Add(empleado);
            await _db.SaveChangesAsync();
            return Ok(empleado);
        }

        // PUT: api/personal/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, Empleado empleado)
        {
            var existente = await _db.Empleados.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = empleado.Nombre;
            existente.Apellido = empleado.Apellido;
            existente.IdPuesto = empleado.IdPuesto; // Cambio de puesto

            await _db.SaveChangesAsync();
            return Ok(existente);
        }

        // DELETE: api/personal/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var existente = await _db.Empleados.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Activo = false; // Soft Delete
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}