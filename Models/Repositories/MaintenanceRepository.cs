using Microsoft.EntityFrameworkCore;
using property_lease_saas.Data;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Models.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContext GetAppDbContext()
    {
        return _context;
    }
    public MaintenanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MaintenanceRequest request)
    {
        _context.MaintenanceRequests.Add(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MaintenanceRequest request)
    {
        _context.MaintenanceRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(Guid id)
    {
        return await _context.MaintenanceRequests
            .Include(r => r.Applications)  // Only include Applications
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<MaintenanceRequest>> GetForTenantAsync(string tenantId)
    {
        return await _context.MaintenanceRequests
            .Include(r => r.Applications)
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MaintenanceRequest>> GetForLandlordAsync(string landlordId)
    {
        return await _context.MaintenanceRequests
            .Include(r => r.Applications)
            .Where(r => r.LandlordId == landlordId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MaintenanceRequest>> GetPublishedAsync()
    {
        return await _context.MaintenanceRequests
            .Where(r => r.Status == MaintenanceRequestStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MaintenanceRequest>> GetForMechanicAsync(string mechanicId)
    {
        return await _context.MaintenanceRequests
            .Include(r => r.Applications)
            .Where(r => r.AssignedMechanicId == mechanicId || 
                        r.Applications.Any(a => a.MechanicId == mechanicId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}