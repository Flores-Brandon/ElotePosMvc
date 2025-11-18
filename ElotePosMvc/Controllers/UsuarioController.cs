using ElotePosMvc.Data;
using ElotePosMvc.Models;
using ElotePosMvc.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class UsuarioController : Controller
{
    private readonly ColdDbContext _coldDb;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuarioController(ColdDbContext coldDbContext, IPasswordHasher<Usuario> passwordHasher)
    {
        _coldDb = coldDbContext;
        _passwordHasher = passwordHasher;
    }

    // El Jefe crea un nuevo empleado
    [HttpPost]
    public async Task<IActionResult> CrearEmpleado(NuevoEmpleadoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var nuevoUsuario = new Usuario
        {
            NombreCompleto = model.NombreCompleto,
            Username = model.Username,
            IdRol = model.IdRol, // (Asumiendo que 2 = "Empleado")
            Activo = true
        };

        // ¡LA MAGIA!
        // Creamos el hash a partir de la contraseña del modelo
        var hashedPassword = _passwordHasher.HashPassword(nuevoUsuario, model.Password);

        // Guardamos el HASH, no la contraseña
        nuevoUsuario.PasswordHash = hashedPassword;

        _coldDb.Usuarios.Add(nuevoUsuario);
        await _coldDb.SaveChangesAsync();

        return RedirectToAction("ListaEmpleados");
    }
}