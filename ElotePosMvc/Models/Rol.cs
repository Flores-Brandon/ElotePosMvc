using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models
{
    // 👇 AQUÍ ESTÁ EL CAMBIO: Heredamos de EntidadAuditable
    public class Rol : EntidadAuditable
    {
        [Key]
        public int IdRol { get; set; }

        public string Nombre { get; set; }

        // Un Rol puede tener muchos Usuarios
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}