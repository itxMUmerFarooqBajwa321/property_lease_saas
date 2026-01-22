using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace property_lease_saas.Models.Entities;

// Payment from Tenant to Landlord for Rent
public class RentPayment
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public Guid LeaseId { get; set; } 
    [ForeignKey("LeaseId")]
    public Lease? Lease { get; set; }

    [Required]
    public string TenantId { get; set; } = "";
    public string TenantName { get; set; } = "";

    [Required]
    public string LandlordId { get; set; } = "";
    public string LandlordName { get; set; } = "";

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;
    
    public DateTime DueDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = ""; // Cash, BankTransfer, Card, MobileMoney

    [MaxLength(100)]
    public string TransactionId { get; set; } = ""; // Bank/Payment gateway transaction ID

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(500)]
    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string PaymentFor { get; set; } = ""; // e.g., "January 2024", "February 2024"

    public bool IsLate { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LateFee { get; set; } = 0;
}