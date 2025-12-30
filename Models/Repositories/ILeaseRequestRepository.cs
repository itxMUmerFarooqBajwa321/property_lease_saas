using property_lease_saas.Models.Entities;
public interface ILeaseRequestRepository
{
    Task AddAsync(LeaseRequest request);
    Task<LeaseRequest?> GetByIdAsync(Guid id);
    Task<List<LeaseRequest>> GetForLandlordAsync(string landlordId);
    Task<List<LeaseRequest>> GetForTenantAsync(string tenantId);
    Task SaveAsync();
    Task<bool> ExistsAsync(Guid propertyId, string tenantId);

}
