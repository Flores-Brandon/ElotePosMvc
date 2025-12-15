using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Departamentos")]
    public class Departamento
    {
        [Key]
        public int IdDepartamento { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}