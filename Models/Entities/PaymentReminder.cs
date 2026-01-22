using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace property_lease_saas.Models.Entities;
public class PaymentReminder
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public Guid LeaseId { get; set; }
    [ForeignKey("LeaseId")]
    public Lease? Lease { get; set; }

    [Required]
    public string TenantId { get; set; } = "";

    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public bool IsSent { get; set; } = false;

    public DateTime? SentAt { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}