using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    public class HotDbContext : DbContext
    {
        public HotDbContext(DbContextOptions<HotDbContext> options) : base(options)
        {
        }

        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Turno> Turnos { get; set; }

        // 👇 ESTA ES LA SOLUCIÓN MÁGICA 👇
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Venta>(entity =>
            {
                // 1. Aseguramos que use la tabla correcta
                entity.ToTable("Ventas");

                // 2. Aseguramos la llave primaria
                entity.HasKey(e => e.IdVenta);

                // 3. 🛑 AQUÍ CORREGIMOS EL ERROR 🛑
                // Le decimos: "La propiedad IdTurno va a la columna 'IdTurno', NO a 'TurnoIdTurno'"
                entity.Property(e => e.IdTurno).HasColumnName("IdTurno");

                // Mapeamos el resto por si acaso
                entity.Property(e => e.IdUsuario).HasColumnName("IdUsuario");
                entity.Property(e => e.FechaHora).HasColumnName("FechaHora");
                entity.Property(e => e.TotalVenta).HasColumnName("TotalVenta");

                // Ignoramos cualquier relación automática con Turnos por ahora
                // para que no busque llaves foráneas que no existen
            });

            // --- CONFIGURACIÓN DE TURNOS (NUEVO) ---
            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turnos");
                entity.HasKey(e => e.IdTurno);

                // Mapeo explícito de columnas importantes
                entity.Property(e => e.FechaInicio).HasColumnName("FechaInicio");
                entity.Property(e => e.FechaCierre).HasColumnName("FechaCierre");
                entity.Property(e => e.IdUsuarioAbre).HasColumnName("IdUsuarioAbre");
                entity.Property(e => e.IdUsuarioCierra).HasColumnName("IdUsuarioCierra");
                entity.Property(e => e.SaldoInicial).HasColumnName("SaldoInicial");
            });
        }
    }
}