using System;
using property_lease_saas.Data;
namespace property_lease_saas.Models.Entities;
public class PropertyImage
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }

    public string FilePath { get; set; }
    public DateTime UploadedAt { get; set; }

    public Property Property { get; set; }
}
