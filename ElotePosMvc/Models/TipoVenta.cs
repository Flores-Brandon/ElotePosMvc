namespace ElotePosMvc.Models
{
    public class TipoVenta
    {
        public int IdTipoVenta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EsRegalado { get; set; }
        public bool Activo { get; set; } = true; // Agregar esta propiedad
    }
}