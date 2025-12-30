using Microsoft.EntityFrameworkCore;
using property_lease_saas.Data;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Models.Repositories;

public class LeaseRepository
{
    private readonly ApplicationDbContext _context;

    public LeaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Lease lease)
    {
        await _context.Leases.AddAsync(lease);
        await _context.SaveChangesAsync();
    }

    public async Task<Lease?> GetByIdAsync(Guid id)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Lease>> GetForLandlordAsync(string landlordId)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .Where(l => l.LandlordId == landlordId)
            .ToListAsync();
    }

    public async Task<List<Lease>> GetForTenantAsync(string tenantId)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .Where(l => l.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Lease lease)
    {
        _context.Leases.Update(lease);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(Guid propertyId, string tenantId)
    {
        return await _context.Leases
            .AnyAsync(l =>
                l.PropertyId == propertyId &&
                l.TenantId == tenantId
            );
    }

}
