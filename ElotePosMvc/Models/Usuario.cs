using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <--- NECESARIO PARA EL JSON IGNORE

namespace ElotePosMvc.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public string NombreCompleto { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public bool Activo { get; set; }

        // La llave foránea (El número)
        public int IdRol { get; set; }

        // La relación (El objeto)
        [ForeignKey("IdRol")]
        [JsonIgnore]
        public virtual Rol? Rol { get; set; }
    }
}