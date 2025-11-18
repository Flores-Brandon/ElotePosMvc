using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        public string Nombre { get; set; }

        // Un Rol puede tener muchos Usuarios
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}