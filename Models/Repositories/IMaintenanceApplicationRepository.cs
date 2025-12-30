using property_lease_saas.Data;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Models.Repositories;

public interface IMaintenanceApplicationRepository
{
    Task AddAsync(MaintenanceApplication app);
    Task UpdateAsync(MaintenanceApplication app);
    Task<MaintenanceApplication?> GetByIdAsync(Guid id);

    Task<List<MaintenanceApplication>> ForRequestAsync(Guid requestId);
    Task<List<MaintenanceApplication>> ForMechanicAsync(string mechanicId);
}
