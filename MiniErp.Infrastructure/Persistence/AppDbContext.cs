
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniErp.Domain.Entities;
using System;

namespace MiniErp.Infrastructure.Persistence
{
    /// <summary>
    /// DbContext que integra Identity + entidades de dominio.
    /// </summary>
    public class AppDbContext
        : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Configuraciones de mapping (relaciones, restricciones).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relación 1..N: Order -> OrderItems
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reglas simples
            builder.Entity<Order>()
                .Property(o => o.CustomerName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Entity<OrderItem>()
                .Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
