namespace ElotePosMvc.Models
{
    // Datos para ABRIR caja
    public class AbrirCajaDto
    {
        public decimal SaldoInicial { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    // Datos para CERRAR caja
    public class CerrarCajaDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ResumenTurnoDto
    {
        public int IdTurno { get; set; }
        public DateTime FechaInicio { get; set; }
        public decimal SaldoInicial { get; set; }

        // Desglose de Ventas
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalRegalos { get; set; }

        // Totales Finales
        public decimal TotalVendido { get; set; } // Suma de todo
        public decimal DineroEnCaja { get; set; } // Inicial + Efectivo
    }
}