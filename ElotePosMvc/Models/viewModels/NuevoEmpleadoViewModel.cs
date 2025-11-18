using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models.ViewModels
{
    public class NuevoEmpleadoViewModel
    {
        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public int IdRol { get; set; } // El Jefe elegirá un Rol
    }
}