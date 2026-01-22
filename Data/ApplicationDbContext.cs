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
        public DbSet<RentPayment> RentPayments { get; set; }
        public DbSet<MaintenancePayment> MaintenancePayments { get; set; }
        public DbSet<PaymentReminder> PaymentReminders { get; set; }

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
            // MAINTENANCE CONFIGURATIONS - UPDATED
            // ===========================================
            
            // MaintenanceRequest to Property (REQUIRED)
            builder.Entity<MaintenanceRequest>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);  // ✅ Changed to required

            // MaintenanceRequest to Lease (OPTIONAL)
            builder.Entity<MaintenanceRequest>()
                .HasOne<Lease>()
                .WithMany()
                .HasForeignKey(r => r.LeaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // MaintenanceRequest to Landlord (REQUIRED)
            builder.Entity<MaintenanceRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.LandlordId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);  // ✅ Added

            // MaintenanceRequest to Tenant (OPTIONAL)
            builder.Entity<MaintenanceRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);  // ✅ Added

            // MaintenanceRequest to AssignedMechanic (OPTIONAL)
            builder.Entity<MaintenanceRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.AssignedMechanicId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);  // ✅ Added

            // MaintenanceApplication to MaintenanceRequest
            builder.Entity<MaintenanceRequest>()
                .HasMany(r => r.Applications)
                .WithOne(a => a.MaintenanceRequest)
                .HasForeignKey(a => a.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Maintenance indexes
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.Status);
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.PropertyId);
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.LeaseId);

            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.LandlordId);  // ✅ Added
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.TenantId);  // ✅ Added
                
            builder.Entity<MaintenanceRequest>()
                .HasIndex(r => r.AssignedMechanicId);  // ✅ Added
            
        builder.Entity<RentPayment>()
            .HasOne(rp => rp.Lease)
            .WithMany()
            .HasForeignKey(rp => rp.LeaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure MaintenancePayment relationships
        builder.Entity<MaintenancePayment>()
            .HasOne(mp => mp.MaintenanceRequest)
            .WithMany()
            .HasForeignKey(mp => mp.MaintenanceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenancePayment>()
            .HasOne(mp => mp.MaintenanceApplication)
            .WithMany()
            .HasForeignKey(mp => mp.MaintenanceApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure PaymentReminder relationships
        builder.Entity<PaymentReminder>()
            .HasOne(pr => pr.Lease)
            .WithMany()
            .HasForeignKey(pr => pr.LeaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for better query performance
        builder.Entity<RentPayment>()
            .HasIndex(rp => rp.TenantId);

        builder.Entity<RentPayment>()
            .HasIndex(rp => rp.LandlordId);

        builder.Entity<RentPayment>()
            .HasIndex(rp => rp.Status);

        builder.Entity<MaintenancePayment>()
            .HasIndex(mp => mp.MechanicId);

        builder.Entity<MaintenancePayment>()
            .HasIndex(mp => mp.LandlordId);

        builder.Entity<MaintenancePayment>()
            .HasIndex(mp => mp.Status);
       

        }       
    }
}