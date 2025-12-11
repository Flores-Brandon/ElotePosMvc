using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    // 👇 AQUÍ ESTÁ EL CAMBIO: Heredamos de EntidadAuditable
    public class Producto : EntidadAuditable
    {
        [Key]
        public int IdProducto { get; set; }

        public string Nombre { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioVenta { get; set; }

        // NO agregues IdUsuarioCreacion aquí, ya viene "gratis" por la herencia.
    }
}