using System.ComponentModel.DataAnnotations;

namespace property_lease_saas.Models.Entities;

public class Lease
{
    [Key]
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }
    public Property Property { get; set; }

    public string TenantId { get; set; }
    public string LandlordId { get; set; }

    public decimal RentAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public LeaseStatus Status { get; set; } = LeaseStatus.Pending;
}
