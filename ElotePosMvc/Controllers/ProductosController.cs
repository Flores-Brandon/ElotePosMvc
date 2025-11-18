using ElotePosMvc.Data;
using ElotePosMvc.Models; // ¡Importante para que reconozca 'Producto'!
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ¡Importante para .ToListAsync()!
using System.Threading.Tasks; // ¡Importante para 'Task'!

namespace ElotePosMvc.Controllers
{
    // ¡HEREDA DE BASECONTROLLER!
    public class ProductosController : BaseController
    {
        private readonly ColdDbContext _coldDb;

        // --- ¡AQUÍ ESTÁ LA CORRECCIÓN! ---
        // Añadí las llaves { y } que faltaban
        public ProductosController(ColdDbContext coldDbContext)
        { // <-- LLAVE DE APERTURA
            _coldDb = coldDbContext;
        } // <-- LLAVE DE CIERRE

        // --- ACCIÓN 1: LEER (Read) ---
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            var listaDeProductos = await _coldDb.Productos.ToListAsync();
            return View(listaDeProductos);
        }

        // --- ACCIÓN 2: MOSTRAR FORMULARIO DE CREAR (Create GET) ---
        public IActionResult Crear()
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        // --- ACCIÓN 3: GUARDAR EL NUEVO PRODUCTO (Create POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto)
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            if (ModelState.IsValid)
            {
                _coldDb.Productos.Add(producto);
                await _coldDb.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        // --- ACCIÓN 4: MOSTRAR FORMULARIO DE EDITAR (Update GET) ---
        // Se accede con /Productos/Editar/5 (donde 5 es el Id)
        public async Task<IActionResult> Editar(int? id)
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            if (id == null)
            {
                return NotFound(); // Error si no mandan ID
            }

            // Busca el producto en la BD Fría
            var producto = await _coldDb.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound(); // Error si no lo encuentra
            }

            // Manda el producto encontrado a la vista de "Editar"
            return View(producto);
        }

        // --- ACCIÓN 5: GUARDAR LOS CAMBIOS (Update POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto)
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            // Verifica que el ID de la URL coincida con el ID del modelo
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Le dice al DbContext que este producto ha sido modificado
                    _coldDb.Update(producto);
                    await _coldDb.SaveChangesAsync(); // Guarda los cambios
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Manejo de error por si alguien borró el producto
                    // mientras lo estábamos editando.
                    return NotFound();
                }

                // Todo salió bien, vuelve a la lista
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, muestra el formulario de nuevo
            return View(producto);
        }

        // --- ACCIÓN 6: MOSTRAR CONFIRMACIÓN DE BORRADO (Delete GET) ---
        // Se accede con /Productos/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            // Busca el producto y lo muestra
            var producto = await _coldDb.Productos
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // --- ACCIÓN 7: CONFIRMAR BORRADO (Delete POST) ---
        [HttpPost, ActionName("Eliminar")] // Le decimos que esta acción se llama "Eliminar"
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            if (HttpContext.Session.GetString("Rol") != "Jefe")
            {
                return RedirectToAction("Login", "Login");
            }

            // Busca el producto
            var producto = await _coldDb.Productos.FindAsync(id);
            if (producto != null)
            {
                // Lo borra del DbContext
                _coldDb.Productos.Remove(producto);
                await _coldDb.SaveChangesAsync(); // Aplica los cambios
            }

            return RedirectToAction(nameof(Index));
        }
    }
}