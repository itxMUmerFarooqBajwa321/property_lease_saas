using System;
using property_lease_saas.Data;
namespace property_lease_saas.Models.Entities;
public class PropertyDocument
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }

    public string FilePath { get; set; }
    public string DocumentType { get; set; } // e.g. Ownership Deed
    public DateTime UploadedAt { get; set; }

    public Property Property { get; set; }
}
