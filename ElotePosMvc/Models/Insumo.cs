using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Insumos")]
    public class Insumo
    {
        [Key]
        public int IdInsumo { get; set; }

        public string Nombre { get; set; } = null!;

        public string UnidadMedida { get; set; } = null!;

        public int Stock { get; set; }

    }
}