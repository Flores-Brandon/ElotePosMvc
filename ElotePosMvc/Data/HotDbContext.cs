using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    public class HotDbContext : DbContext
    {
        public HotDbContext(DbContextOptions<HotDbContext> options) : base(options)
        {
        }

        // Registra solo los modelos "CALIENTES"
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetallesVenta> DetallesVenta { get; set; }
        public DbSet<InventarioTurno> InventarioTurnos { get; set; }
    }
}