using property_lease_saas.Models.Entities;
namespace property_lease_saas.Models.Repositories;
public interface IPropertyRepository
{
    Task AddAsync(Property property);
    Task<Property> GetByIdAsync(Guid id);
    Task<IEnumerable<Property>> GetByLandlordAsync(string landlordId);
    Task<IEnumerable<Property>> GetAvailableAsync();
    Task SaveAsync();
    Task DeleteAsync(Guid id);

}
