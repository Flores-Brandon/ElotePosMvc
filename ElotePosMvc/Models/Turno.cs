using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <--- Necesario para [Column] y [Table]

namespace ElotePosMvc.Models
{
    [Table("Turnos")] // Aseguramos que busque la tabla "Turnos"
    public class Turno
    {
        [Key]
        [Column("IdTurno")]
        public int IdTurno { get; set; }

        [Column("FechaInicio")]
        public DateTime FechaInicio { get; set; }

        [Column("FechaCierre")]
        public DateTime? FechaCierre { get; set; } // Puede ser nulo

        [Column("IdUsuarioAbre")]
        public int IdUsuarioAbre { get; set; }

        [Column("IdUsuarioCierra")]
        public int? IdUsuarioCierra { get; set; } // Puede ser nulo

        // 👇 ESTA ES LA PROPIEDAD QUE TE FALTABA 👇
        [Column("SaldoInicial")]
        public decimal SaldoInicial { get; set; }
    }
}