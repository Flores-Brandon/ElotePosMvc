using System.ComponentModel.DataAnnotations;

namespace ElotePosMvc.Models
{
    public class Insumo
    {
        [Key]
        public int IdInsumo { get; set; }
        public string Nombre { get; set; }
        public string UnidadMedida { get; set; }
    }
}