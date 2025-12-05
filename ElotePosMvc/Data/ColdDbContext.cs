using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    public class ColdDbContext : DbContext
    {
        public ColdDbContext(DbContextOptions<ColdDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Insumo> Insumos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- USUARIOS ---
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.NombreCompleto).HasColumnName("NombreCompleto");
                entity.Property(e => e.Username).HasColumnName("Username");
                entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
                entity.Property(e => e.IdRol).HasColumnName("IdRol");
                entity.Property(e => e.Activo).HasColumnName("Activo");

                // Relación con Rol
                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.IdRol)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usuarios_Roles");
            });

            // --- ROLES ---
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.IdRol);
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
            });

            // --- PRODUCTOS (Menú) ---
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.PrecioVenta).HasColumnName("PrecioVenta");
                // Si tienes ImagenUrl u otros campos, van aquí
            });

            // --- INSUMOS (Bodega) ---
            modelBuilder.Entity<Insumo>(entity =>
            {
                entity.ToTable("Insumos");
                entity.HasKey(e => e.IdInsumo);

                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.UnidadMedida).HasColumnName("UnidadMedida");
                entity.Property(e => e.Stock).HasColumnName("Stock");

                // 🧹 YA QUITAMOS 'RequiereConteo' PARA QUE NO DE ERROR
            });
        }
    }
}