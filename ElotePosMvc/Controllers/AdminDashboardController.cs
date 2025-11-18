using Microsoft.AspNetCore.Mvc;

namespace ElotePosMvc.Controllers
{
    // ¡LA MAGIA! Hereda de BaseController
    public class AdminDashboardController : BaseController
    {
        public IActionResult Index()
        {
            // Gracias al BaseController, este código SOLO se ejecuta
            // si el usuario está logueado.

            // Ahora, comprobamos el ROL
            string? rol = HttpContext.Session.GetString("Rol");

            // Si un empleado intenta entrar a la URL... ¡lo echamos!
            if (rol != "Jefe")
            {
                // Lo mandamos a la página de login (o a donde quieras)
                return RedirectToAction("Login", "Login");
            }

            // Si llegamos aquí, ES el Jefe.
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto");
            return View();
        }
    }
}