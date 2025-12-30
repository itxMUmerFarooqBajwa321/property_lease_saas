namespace property_lease_saas.Models.Entities;
public class MaintenanceCompletion
{
    public Guid Id { get; set; }
    public Guid MaintenanceRequestId { get; set; }

    public string ReceiptPath { get; set; }
    public string CompletionImagePath { get; set; }

    public DateTime UploadedAt { get; set; }
}
