using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class Usuario
    {
        [Key] // Esto le dice que es la Primary Key
        public int IdUsuario { get; set; }

        public int IdRol { get; set; } // La Foreign Key    
        public string NombreCompleto { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool Activo { get; set; }

        // ¡RELACIÓN FELIZ!
        // Como 'Rol' vive en la MISMA BD (SQL Server),
        // podemos crear una relación de navegación.
        [ForeignKey("IdRol")]
        public virtual Rol Rol { get; set; }
    }
}
