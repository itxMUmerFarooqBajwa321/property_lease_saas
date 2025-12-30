using property_lease_saas.Data;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Models.Repositories;

public interface IMaintenanceRepository
{
    Task AddAsync(MaintenanceRequest request);
    Task UpdateAsync(MaintenanceRequest request);
    Task<MaintenanceRequest?> GetByIdAsync(Guid id);

    Task<List<MaintenanceRequest>> GetForTenantAsync(string tenantId);
    Task<List<MaintenanceRequest>> GetForLandlordAsync(string landlordId);
    Task<List<MaintenanceRequest>> GetPublishedAsync();
    Task<List<MaintenanceRequest>> GetForMechanicAsync(string mechanicId);
}
