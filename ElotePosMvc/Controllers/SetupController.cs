using ElotePosMvc.Data;
using ElotePosMvc.Models;
using ElotePosMvc.Models.ViewModels; // ¡Necesitamos el LoginViewModel!
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ElotePosMvc.Controllers
{
    // ¡OJO! Este NO hereda de BaseController
    // porque necesitamos acceder SIN iniciar sesión.
    public class SetupController : Controller
    {
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public SetupController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
        {
            _coldDb = coldDbContext;
            _passwordHasher = passwordHasher;
        }

        // --- ACCIÓN 1: MOSTRAR FORMULARIO DE CREAR JEFE ---
        [HttpGet]
        public IActionResult CrearPrimerJefe()
        {
            // Pasamos un modelo vacío al formulario
            return View(new LoginViewModel());
        }

        // --- ACCIÓN 2: GUARDAR EL PRIMER JEFE ---
        [HttpPost]
        public async Task<IActionResult> CrearPrimerJefe(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- PASO A: "Sembrar" los Roles ---
            var rolJefe = await _coldDb.Roles.FirstOrDefaultAsync(r => r.Nombre == "Jefe");
            if (rolJefe == null)
            {
                rolJefe = new Rol { Nombre = "Jefe" };
                var rolEmpleado = new Rol { Nombre = "Empleado" };

                _coldDb.Roles.Add(rolJefe);
                _coldDb.Roles.Add(rolEmpleado);
                await _coldDb.SaveChangesAsync();
            }

            // --- PASO B: Revisar si ya existe un Jefe ---
            var primerJefe = await _coldDb.Usuarios.FirstOrDefaultAsync(u => u.IdRol == rolJefe.IdRol);
            if (primerJefe != null)
            {
                ModelState.AddModelError(string.Empty, "Error: Ya existe un usuario 'Jefe' en el sistema.");
                return View(model);
            }

            // --- PASO C: Crear el nuevo usuario Jefe ---
            var nuevoJefe = new Usuario
            {
                NombreCompleto = "Administrador Principal",
                Username = model.Username,
                IdRol = rolJefe.IdRol,
                Activo = true
            };

            nuevoJefe.PasswordHash = _passwordHasher.HashPassword(nuevoJefe, model.Password);

            _coldDb.Usuarios.Add(nuevoJefe);
            await _coldDb.SaveChangesAsync();



            // Usamos TempData en lugar de ViewBag para que el mensaje "sobreviva" a la redirección
            TempData["MensajeExito"] = "¡Éxito! El usuario 'Jefe' ha sido creado. Ya puedes iniciar sesión.";

            // ¡LA CORRECCIÓN! Redirigimos a la acción "Login" del controlador "Login"
            return RedirectToAction("Login", "Login");
        }
    }
}