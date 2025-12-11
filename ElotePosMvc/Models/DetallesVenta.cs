using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    // 🛠️ CORRECCIÓN: Apuntamos a la tabla plural 'detalleventas'
    [Table("detalleventas")]
    public class DetalleVenta
    {
        [Key]
        public int IdDetalle { get; set; }

        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        public int? IdUsuarioCreacion { get; set; }
    }
}