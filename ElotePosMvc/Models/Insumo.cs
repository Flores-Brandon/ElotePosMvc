using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models
{
    public class Insumo
    {
        [Key]
        public int IdInsumo { get; set; }

        public string Nombre { get; set; } = null!;

        public string UnidadMedida { get; set; } = null!;

        public decimal Costo { get; set; }
    }
}