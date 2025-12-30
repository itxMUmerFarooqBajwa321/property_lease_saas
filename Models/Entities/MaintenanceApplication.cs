using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace property_lease_saas.Models.Entities;

public class MaintenanceApplication
{
    [Key]
    public Guid Id { get; set; }

    public Guid MaintenanceRequestId { get; set; }
    [ForeignKey("MaintenanceRequestId")]
    public MaintenanceRequest MaintenanceRequest { get; set; }

    public string MechanicId { get; set; } = null!;
    public decimal ProposedBill { get; set; }
    public string Notes { get; set; } = null!;

    public bool IsAccepted { get; set; }
    public DateTime AppliedAt { get; set; }
}
