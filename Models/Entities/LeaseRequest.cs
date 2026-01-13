using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace property_lease_saas.Models.Entities;
public class LeaseRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PropertyId { get; set; }
    public Property Property { get; set; }

    [Required]
    public string TenantId { get; set; }

    [Required]
    public string LandlordId { get; set; }

    public LeaseRequestStatus Status { get; set; } = LeaseRequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key to Lease (if approved)
    public Guid? LeaseId { get; set; }
    
    // Navigation property
    public virtual Lease? Lease { get; set; }
}

public enum LeaseRequestStatus
{
    Pending,
    Approved,
    Rejected
}