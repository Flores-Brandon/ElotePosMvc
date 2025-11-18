using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class DetallesVenta
    {
        [Key]
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }

        // --- ID Huérfano (de la BD Fría) ---
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitarioHistorico { get; set; }

        // Relación (dentro de la BD Caliente)
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; }
    }
}