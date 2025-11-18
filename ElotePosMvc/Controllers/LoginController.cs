using ElotePosMvc.Data;
using ElotePosMvc.Models;
using ElotePosMvc.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ElotePosMvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly ColdDbContext _coldDb;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public LoginController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
        {
            _coldDb = coldDbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // ¡Revisamos si el usuario ya tiene una sesión activa!
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Rol")))
            {
                // Si la sesión existe, redirigimos a su panel
                if (HttpContext.Session.GetString("Rol") == "Jefe")
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {
                    return RedirectToAction("Index", "EmpleadoPOS");
                }
            }

            // Si no hay sesión, SÍ mostramos la página de login
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _coldDb.Usuarios
                                .Include(u => u.Rol)
                                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (usuario == null || !usuario.Activo)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            // Verificar hash de contraseña
            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            // Guardar sesión simple
            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetString("Rol", usuario.Rol.Nombre);

            // Redirigir según rol
            if (usuario.Rol.Nombre == "Jefe")
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
            else
            {
                return RedirectToAction("Index", "EmpleadoPOS");
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
