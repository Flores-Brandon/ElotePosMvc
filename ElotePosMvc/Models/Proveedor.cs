using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElotePosMvc.Models
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }
        public string Empresa { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public bool Activo { get; set; }
    }
}