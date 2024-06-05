using Microsoft.EntityFrameworkCore;

namespace MyWebAPI.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Presencia> Presencia { get; set; }
        public DbSet<Puestos> Puestos { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Turnos> Turnos { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Zona> Zona { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuraciones adicionales si son necesarias
        }
    }
}
