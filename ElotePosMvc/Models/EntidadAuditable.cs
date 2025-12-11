using System;

namespace ElotePosMvc.Models
{
    public class EntidadAuditable
    {
        // Estos nombres deben coincidir con las columnas que agregamos en SQL Server
        public int? IdUsuarioCreacion { get; set; }
        public DateTime? FechaCreacion { get; set; }

        public int? IdUsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public bool? Activo { get; set; } = true;
    }
}