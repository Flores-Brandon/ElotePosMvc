using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    public class HotDbContext : DbContext
    {
        public HotDbContext(DbContextOptions<HotDbContext> options) : base(options)
        {
        }

        // =========================================================
        // 🟢 ZONA 1: TABLAS NATIVAS DE SQL SERVER (Tus 20 tablas)
        // =========================================================

        // 1. Módulo de Seguridad (Lo que ya tenías)
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; } // Si tienes tabla Roles

        // 2. Módulo de Productos (Lo que ya tenías)
        public DbSet<Producto> Productos { get; set; }

        // 3. Módulo de Recursos Humanos (RRHH) - ¡NUEVO!
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Puesto> Puestos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        // public DbSet<Nomina> Nominas { get; set; } // Descomentar si creas el modelo

        // 4. Módulo de Compras y Proveedores - ¡NUEVO!
        // (Agregamos de una vez Proveedores para el siguiente paso)
        public DbSet<Proveedor> Proveedores { get; set; }
        // public DbSet<Compra> Compras { get; set; }

        // 5. Módulo de Inventario - ¡NUEVO!
        // public DbSet<Insumo> Insumos { get; set; }
        // public DbSet<Categoria> Categorias { get; set; }


        // =========================================================
        // 🟠 ZONA 2: VISTAS LINKED SERVER (Datos de MySQL)
        // =========================================================

        // Estas no son tablas reales en SQL Server, son ventanas hacia MySQL
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; } // Plural en el DbSet es mejor práctica


        // =========================================================
        // ⚙️ CONFIGURACIÓN DE MAPEO
        // =========================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuración MySQL (Vistas) ---

            modelBuilder.Entity<Turno>()
                .ToView("v_TurnosMysql") // Mapea a la Vista
                .HasKey(t => t.IdTurno);

            modelBuilder.Entity<Venta>()
                .ToView("v_VentasMysql")
                .HasKey(v => v.IdVenta);

            modelBuilder.Entity<DetalleVenta>()
                .ToView("v_DetalleVentaMysql")
                .HasKey(dv => dv.IdDetalle);

            // --- Configuración SQL Server (Tablas Nativas) ---
            // Entity Framework suele detectar las tablas automáticamente por el nombre del DbSet,
            // pero si quieres asegurar nombres específicos, puedes hacerlo aquí:

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Producto>().ToTable("Productos");

            // Nuevas tablas
            modelBuilder.Entity<Departamento>().ToTable("Departamentos");
            modelBuilder.Entity<Puesto>().ToTable("Puestos");
            modelBuilder.Entity<Empleado>().ToTable("Empleados");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedores");
        }
    }
}