using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sklad.Domain.Models;

namespace Sklad.Persistence
{
    public class SkladDbContext : DbContext
    {
        public SkladDbContext(DbContextOptions<SkladDbContext> options) : base(options)
        {
        }

        public DbSet<Resource> Resources { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
        public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
        public DbSet<ReceiptResource> ReceiptResources { get; set; }
        public DbSet<ShipmentResource> ShipmentResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Unit>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<ReceiptDocument>()
                .HasIndex(g => g.Number)
                .IsUnique();

            modelBuilder.Entity<ShipmentDocument>()
                .HasIndex(g => g.Number)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
