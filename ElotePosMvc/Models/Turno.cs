using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models
{
    public class Turno
    {
        [Key]
        public int IdTurno { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaCierre { get; set; } // El '?' permite valores nulos

        // --- IDs Huérfanos (de la BD Fría) ---
        public int IdUsuarioAbre { get; set; }
        public int? IdUsuarioCierra { get; set; } // El '?' permite nulos

        // Relaciones dentro de la BD Caliente
        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<InventarioTurno> InventarioTurnos { get; set; }
    }
}