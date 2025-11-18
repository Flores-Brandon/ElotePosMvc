using ElotePosMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ElotePosMvc.Data
{
    public class ColdDbContext : DbContext
    {
        public ColdDbContext(DbContextOptions<ColdDbContext> options) : base(options)
        {
        }

        // Registra solo los modelos "FRÍOS"
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Receta> Recetas { get; set; }
    }
}