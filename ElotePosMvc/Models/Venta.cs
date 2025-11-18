using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        public int IdTurno { get; set; }

        // ¡LA RELACIÓN ROTA!
        // Este es el ID del 'Usuario' que vive en SQL Server.
        // NO podemos tener un 'public virtual Usuario Usuario { get; set; }'
        // Es solo un número huérfano.
        public int IdUsuario { get; set; }

        public DateTime FechaHora { get; set; }
        public decimal TotalVenta { get; set; }
        public bool EsRegalado { get; set; }

        // ¡RELACIÓN FELIZ!
        // 'Turno' vive en la MISMA BD (MySQL), así que esta sí funciona.
        [ForeignKey("IdTurno")]
        public virtual Turno Turno { get; set; }

        // ... etc
    }
}
