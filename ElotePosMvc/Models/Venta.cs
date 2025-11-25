using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Ventas")]
    public class Venta
    {
        [Key]
        [Column("IdVenta")]
        public int IdVenta { get; set; }

        [Column("TotalVenta")]
        public decimal TotalVenta { get; set; }

        [Column("PagoRecibido")]
        public decimal PagoRecibido { get; set; }

        [Column("CambioDado")]
        public decimal CambioDado { get; set; }

        [Column("EsRegalado")]
        public bool EsRegalado { get; set; }

        [Column("FechaHora")]   
        public DateTime FechaHora { get; set; }

        [Column("IdUsuario")]
        public int IdUsuario { get; set; }

        [Column("IdTurno")]
        public int IdTurno { get; set; }

        [Column("MetodoPago")]
        public string MetodoPago { get; set; } = "Efectivo";
    }
}