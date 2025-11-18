using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    public class Receta
    {
        [Key]
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public int IdInsumo { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CantidadUsada { get; set; }

        // Navegación (para la BD Fría)
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("IdInsumo")]
        public virtual Insumo Insumo { get; set; }
    }
}