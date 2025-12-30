using System.ComponentModel.DataAnnotations;

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
}

public enum LeaseRequestStatus
{
    Pending,
    Approved,
    Rejected
}
