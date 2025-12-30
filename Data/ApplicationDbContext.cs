using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Models.Entities;
using property_lease_saas.Models;

namespace property_lease_saas.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Existing DbSets
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyDocument> PropertyDocuments { get; set; }

        // ADD Lease related DbSets
        public DbSet<Lease> Leases { get; set; }
        public DbSet<LeaseRequest> LeaseRequests { get; set; }

        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceApplication> MaintenanceApplications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Property
    builder.Entity<Property>()
        .HasMany(p => p.Images)
        .WithOne(i => i.Property)
        .HasForeignKey(i => i.PropertyId);

    builder.Entity<Property>()
        .HasMany(p => p.Documents)
        .WithOne(d => d.Property)
        .HasForeignKey(d => d.PropertyId);

    // Lease
    builder.Entity<Lease>()
        .HasOne(l => l.Property)
        .WithMany()
        .HasForeignKey(l => l.PropertyId)
        .IsRequired(false);

    builder.Entity<LeaseRequest>()
        .HasOne(lr => lr.Property)
        .WithMany()
        .HasForeignKey(lr => lr.PropertyId);

    // Maintenance
    builder.Entity<MaintenanceRequest>()
        .HasMany(r => r.Applications)
        .WithOne(a => a.MaintenanceRequest)
        .HasForeignKey(a => a.MaintenanceRequestId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<MaintenanceRequest>()
        .HasOne<Lease>()
        .WithMany()
        .HasForeignKey(r => r.LeaseId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<MaintenanceRequest>()
        .HasOne<Property>()
        .WithMany()
        .HasForeignKey(r => r.PropertyId)
        .OnDelete(DeleteBehavior.Restrict);
        }       
    }
}
