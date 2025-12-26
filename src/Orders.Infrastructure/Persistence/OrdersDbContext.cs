using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);
                e.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
                e.Property(x => x.OrderDate).IsRequired();
            });
        }
    }
}
