using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace property_lease_saas.Models.Entities;
public class MaintenancePayment
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public Guid MaintenanceRequestId { get; set; }
    [ForeignKey("MaintenanceRequestId")]
    public MaintenanceRequest? MaintenanceRequest { get; set; }

    [Required]
    public Guid MaintenanceApplicationId { get; set; } 
    [ForeignKey("MaintenanceApplicationId")]
    public MaintenanceApplication? MaintenanceApplication { get; set; }

    [Required]
    public string MechanicId { get; set; } = "";
    public string MechanicName { get; set; } = "";

    [Required]
    public string LandlordId { get; set; } = "";
    public string LandlordName { get; set; } = "";

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = ""; // Cash, BankTransfer, Card

    [MaxLength(100)]
    public string TransactionId { get; set; } = "";

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(500)]
    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string WorkDescription { get; set; } = "";
}