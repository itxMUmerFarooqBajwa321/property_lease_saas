using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace property_lease_saas.Models.Entities
{
    public class Lease
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

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentAmount { get; set; }
        
        public LeaseStatus Status { get; set; } = LeaseStatus.Active;
        
        // Foreign key to LeaseRequest (optional)
        public Guid? LeaseRequestId { get; set; }
        
        // Navigation property
        public virtual LeaseRequest? LeaseRequest { get; set; }
    }

    public enum LeaseStatus
    {
        Pending,
        Approved,
        Rejected,
        Active,
        Completed
    }
}