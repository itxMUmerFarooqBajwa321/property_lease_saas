using property_lease_saas.Models.Entities;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Data;
namespace property_lease_saas.Services;
public class PropertyService
{
    private readonly IPropertyRepository _repo;
    private readonly IFileStorage _storage;

    public PropertyService(IPropertyRepository repo, IFileStorage storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task CreateAsync(Property property,IEnumerable<IFormFile> images,IEnumerable<IFormFile> documents)
    {
        property.Id = Guid.NewGuid();
        property.CreatedAt = DateTime.UtcNow;

        property.Images = new List<PropertyImage>();
        property.Documents = new List<PropertyDocument>();

        foreach (var img in images)
        {
            var path = await _storage.SaveAsync(img, "property-images");
            property.Images.Add(new PropertyImage
            {
                Id = Guid.NewGuid(),
                FilePath = path
            });
        }

        foreach (var doc in documents)
        {
            var path = await _storage.SaveAsync(doc, "property-docs");
            property.Documents.Add(new PropertyDocument
            {
                Id = Guid.NewGuid(),
                FilePath = path,
                DocumentType = "Ownership"
            });
        }

        await _repo.AddAsync(property);
    }
}
