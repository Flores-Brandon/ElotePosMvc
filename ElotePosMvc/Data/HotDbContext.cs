using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    // Este contexto maneja las operaciones de ALTA FRECUENCIA (HOT DATA)
    // Estas entidades se mapean a las tablas de MySQL a través de Vistas de SQL Server
    // (o directamente si usas el Linked Server como puente).
    public class HotDbContext : DbContext
    {
        public HotDbContext(DbContextOptions<HotDbContext> options) : base(options)
        {
        }

        // --- Entidades Mapeadas a MySQL (a través de Linked Server) ---

        // La tabla 'turnos' de MySQL
        public DbSet<Turno> Turnos { get; set; }

        // La tabla 'ventas' de MySQL
        public DbSet<Venta> Ventas { get; set; }

        // La tabla 'detalleventa' de MySQL
        // Nota: Si solo usas esta tabla para lectura de reportes, podrías usar HasNoKey().
        // Si no la usas en EF Core, podrías omitirla o mapearla.
        public DbSet<DetalleVenta> DetalleVenta { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de Turno
            modelBuilder.Entity<Turno>()
                // Mapeo a la vista o tabla que expone los datos de MySQL en SQL Server
                .ToView("v_TurnosMysql") // Asume que creaste una Vista en SQL Server
                .HasKey(t => t.IdTurno);

            // 2. Configuración de Venta
            modelBuilder.Entity<Venta>()
                .ToView("v_VentasMysql") // Asume que creaste una Vista en SQL Server
                .HasKey(v => v.IdVenta);

            // 3. Configuración de DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .ToView("v_DetalleVentaMysql") // Si usas vista
                .HasKey(dv => dv.IdDetalle);   // <--- OJO: Debe ser dv.IdDetalle
        }
    }
}