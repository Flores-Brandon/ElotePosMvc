using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class InventarioTurno
    {
        [Key]
        public int IdInventarioTurno { get; set; }
        public int IdTurno { get; set; }

        // --- ID Huérfano (de la BD Fría) ---
        public int IdInsumo { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CantidadInicial { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? CantidadFinal { get; set; } // Permite nulos

        // Relación (dentro de la BD Caliente)
        [ForeignKey("IdTurno")]
        public virtual Turno Turno { get; set; }
    }
}