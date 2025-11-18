using Microsoft.AspNetCore.Mvc;

namespace ElotePosMvc.Controllers
{
    // ¡LA MAGIA! Hereda de BaseController
    public class EmpleadoPOSController : BaseController
    {
        public IActionResult Index()
        {
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto");
            return View();
        }
    }
}