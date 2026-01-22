using System.ComponentModel.DataAnnotations;

namespace property_lease_saas.Models.Entities;

public class MaintenanceRequest
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid? LeaseId { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    
    public string? TenantId { get; set; }
    
    [Required]
    public string LandlordId { get; set; }
    
    public string? AssignedMechanicId { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public MaintenanceRequestStatus Status { get; set; }
    
    public string? ReceiptPath { get; set; }
    public string? CompletionImagePath { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Only keep the Applications collection
    public List<MaintenanceApplication> Applications { get; set; } = new();
    
    // ❌ DO NOT include these navigation properties:
    // public Property Property { get; set; }
    // public ApplicationUser? Tenant { get; set; }
    // public ApplicationUser Landlord { get; set; }
    // public ApplicationUser? AssignedMechanic { get; set; }
}