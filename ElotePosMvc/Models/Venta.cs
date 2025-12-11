using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        public int IdTurno { get; set; }
        public int IdUsuario { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalVenta { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PagoRecibido { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CambioDado { get; set; }

        public DateTime FechaHora { get; set; }

        // --- CAMBIO: AHORA USAMOS IDs ---

        public int IdFormaPago { get; set; } // <--- ¡Esto explota si recibe un NULL!

        public int IdTipoVenta { get; set; } // 1=Normal, 2=Regalo

        // DTO Detalle (No cambia)
        [NotMapped]
        public List<DetalleVentaDto>? Productos { get; set; }

        public class DetalleVentaDto
        {
            public int IdProducto { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }
        }
    }
}