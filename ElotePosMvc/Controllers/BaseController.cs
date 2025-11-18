using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ElotePosMvc.Controllers
{
    // Este es un controlador especial del que heredarán otros
    public class BaseController : Controller
    {
        // Esto se ejecuta ANTES de cualquier acción en los controladores hijos
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Revisamos si el "Rol" existe en la sesión
            string? rol = context.HttpContext.Session.GetString("Rol");

            // 2. Si es nulo (no está logueado)...
            if (string.IsNullOrEmpty(rol))
            {
                // 3. ...¡Lo echamos a la página de Login!
                context.Result = new RedirectToActionResult("Login", "Login", null);
            }

            // Si SÍ existe, simplemente continúa
            base.OnActionExecuting(context);
        }
    }
}