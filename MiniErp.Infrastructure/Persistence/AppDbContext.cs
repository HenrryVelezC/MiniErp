using Microsoft.AspNetCore.Identity; // Identity: roles, usuarios, claims
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // DbContext base que integra Identity
using Microsoft.EntityFrameworkCore; // EF Core
using MiniErp.Domain.Entities; // Entidades de dominio
using System; // Tipos base (Guid, etc.)

namespace MiniErp.Infrastructure.Persistence // Capa Infrastructure: persistencia de datos
{
    /// <summary>
    /// DbContext que integra Identity + entidades de dominio.
    /// </summary>
    public class AppDbContext // DbContext principal de la aplicación
        : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid> // Hereda Identity con claves Guid
    {
        public DbSet<Customer> Customers { get; set; } // Tabla Customers
        public DbSet<Order> Orders => Set<Order>(); // Tabla Orders (acceso tipado)
        public DbSet<OrderItem> OrderItems => Set<OrderItem>(); // Tabla OrderItems

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // Constructor con DI

        /// <summary>
        /// Configuraciones de mapping (relaciones, restricciones).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder) // Configuración del modelo EF
        {
            base.OnModelCreating(builder); // Configuración base de Identity

            // Relación 1..N: Order -> OrderItems
            builder.Entity<Order>() // Entidad Order
                .HasMany(o => o.Items) // Un Order tiene muchos Items
                .WithOne() // OrderItem no expone navegación inversa
                .HasForeignKey(i => i.OrderId) // FK OrderId en OrderItem
                .OnDelete(DeleteBehavior.Cascade); // Borrado en cascada

            builder.Entity<Order>() // Entidad Order
                .Property(o => o.CustomerId) // Propiedad CustomerId
                .IsRequired(); // No permite NULL (orden siempre pertenece a un cliente)

            builder.Entity<Order>() // Entidad Order
                .HasIndex(o => o.CustomerId); // Índice para búsquedas por cliente

            // Reglas simples
            builder.Entity<Order>() // Entidad Order
                .Property(o => o.CustomerNameSnapshot) // Snapshot del nombre del cliente
                .HasMaxLength(200) // Longitud máxima
                .IsRequired(); // Obligatorio

            builder.Entity<OrderItem>() // Entidad OrderItem
                .Property(i => i.ProductName) // Nombre del producto
                .HasMaxLength(200) // Longitud máxima
                .IsRequired(); // Obligatorio
        }
    }
}
