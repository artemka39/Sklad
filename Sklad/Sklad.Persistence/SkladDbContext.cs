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
        public DbSet<UnitOfMeasurement> UnitOfMeasurements { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<GoodsIssueDocument> GoodsIssueDocuments { get; set; }
        public DbSet<GoodsReceiptDocument> GoodsReceiptDocuments { get; set; }
        public DbSet<InboundResource> InboundResources { get; set; }
        public DbSet<OutboundResource> OutboundResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<UnitOfMeasurement>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<GoodsReceiptDocument>()
                .HasIndex(g => g.Number)
                .IsUnique();

            modelBuilder.Entity<GoodsIssueDocument>()
                .HasIndex(g => g.Number)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
