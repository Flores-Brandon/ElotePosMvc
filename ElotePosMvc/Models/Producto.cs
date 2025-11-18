using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }
        public string Nombre { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioVenta { get; set; }
    }
}