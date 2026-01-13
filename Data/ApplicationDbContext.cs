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

        // Lease related DbSets
        public DbSet<Lease> Leases { get; set; }
        public DbSet<LeaseRequest> LeaseRequests { get; set; }

        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceApplication> MaintenanceApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===========================================
            // PROPERTY CONFIGURATIONS
            // ===========================================
            builder.Entity<Property>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Property)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Property>()
                .HasMany(p => p.Documents)
                .WithOne(d => d.Property)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===========================================
            // LEASE REQUEST CONFIGURATIONS
            // ===========================================
            builder.Entity<LeaseRequest>()
                .HasOne(lr => lr.Property)
                .WithMany()
                .HasForeignKey(lr => lr.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaseRequest>()
                .HasOne(lr => lr.Lease)
                .WithOne(l => l.LeaseRequest)
                .HasForeignKey<Lease>(l => l.LeaseRequestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ===========================================
            // LEASE CONFIGURATIONS
            // ===========================================
            builder.Entity<Lease>()
                .HasOne(l => l.Property)
                .WithMany()
                .HasForeignKey(l => l.PropertyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lease>()
                .HasOne(l => l.LeaseRequest)
                .WithOne(lr => lr.Lease)
                .HasForeignKey<LeaseRequest>(lr => lr.LeaseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ===========================================
            // INDEXES FOR PERFORMANCE
            // ===========================================
            // LeaseRequest indexes
            builder.Entity<LeaseRequest>()
                .HasIndex(lr => lr.TenantId);
                
            builder.Entity<LeaseRequest>()
                .HasIndex(lr => lr.LandlordId);
                
            builder.Entity<LeaseRequest>()
                .HasIndex(lr => lr.PropertyId);
                
            builder.Entity<LeaseRequest>()
                .HasIndex(lr => lr.Status);
                
            builder.Entity<LeaseRequest>()
                .HasIndex(lr => lr.RequestedAt);

            // Lease indexes
            builder.Entity<Lease>()
                .HasIndex(l => l.TenantId);
                
            builder.Entity<Lease>()
                .HasIndex(l => l.LandlordId);
                
            builder.Entity<Lease>()
                .HasIndex(l => l.PropertyId);
                
            builder.Entity<Lease>()
                .HasIndex(l => l.Status);
                
            builder.Entity<Lease>()
                .HasIndex(l => l.StartDate);
                
            builder.Entity<Lease>()
                .HasIndex(l => l.EndDate);

            // Property indexes
            builder.Entity<Property>()
                .HasIndex(p => p.LandlordId);
                
            builder.Entity<Property>()
                .HasIndex(p => p.IsPublished);
                
            builder.Entity<Property>()
                .HasIndex(p => p.IsTaken);

            // ===========================================
            // MAINTENANCE CONFIGURATIONS
            // ===========================================
            builder.Entity<MaintenanceRequest>()
                .HasMany(r => r.Applications)
                .WithOne(a => a.MaintenanceRequest)
                .HasForeignKey(a => a.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MaintenanceRequest>()
                .HasOne<Lease>()
                .WithMany()
                .HasForeignKey(r => r.LeaseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaintenanceRequest>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Maintenance indexes
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.Status);
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.PropertyId);
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.LeaseId);
        }       
    }
}