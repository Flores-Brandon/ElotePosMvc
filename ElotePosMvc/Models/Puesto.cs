using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Puestos")]
    public class Puesto
    {
        [Key]
        public int IdPuesto { get; set; }
        public string Nombre { get; set; }
        public decimal SalarioBase { get; set; }

        public int IdDepartamento { get; set; }

        [ForeignKey("IdDepartamento")]
        public Departamento Departamento { get; set; } // Para la relación
    }
}