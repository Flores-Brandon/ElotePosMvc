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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            // --- VENTAS ---
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("Ventas");
                entity.HasKey(e => e.IdVenta);
                entity.Property(e => e.IdTurno).HasColumnName("IdTurno");
                entity.Property(e => e.IdUsuario).HasColumnName("IdUsuario");
                entity.Property(e => e.FechaHora).HasColumnName("FechaHora");
                entity.Property(e => e.TotalVenta).HasColumnName("TotalVenta");
                entity.Property(e => e.PagoRecibido).HasColumnName("PagoRecibido");
                entity.Property(e => e.CambioDado).HasColumnName("CambioDado");
                entity.Property(e => e.MetodoPago).HasColumnName("MetodoPago");
                entity.Property(e => e.EsRegalado).HasColumnName("EsRegalado");
            });

            // --- TURNOS ---
            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turnos");
                entity.HasKey(e => e.IdTurno);
                entity.Property(e => e.FechaInicio).HasColumnName("FechaInicio");
                entity.Property(e => e.FechaCierre).HasColumnName("FechaCierre");
                entity.Property(e => e.IdUsuarioAbre).HasColumnName("IdUsuarioAbre");
                entity.Property(e => e.IdUsuarioCierra).HasColumnName("IdUsuarioCierra");
                entity.Property(e => e.SaldoInicial).HasColumnName("SaldoInicial");
            });
        }
    }
}