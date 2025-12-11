namespace ElotePosMvc.Models
{
    public class FormaPago
    {
        public int IdFormaPago { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true; // Agregar esta propiedad
    }
}