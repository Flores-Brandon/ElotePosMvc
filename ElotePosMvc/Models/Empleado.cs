using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Empleados")]
    public class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaContratacion { get; set; }
        public bool Activo { get; set; }

        public int IdPuesto { get; set; }

        [ForeignKey("IdPuesto")]
        public Puesto Puesto { get; set; } // Para unir con Puesto y Depto
    }
}