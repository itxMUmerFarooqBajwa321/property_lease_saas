using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Models.Entities;
using property_lease_saas.Models.Repositories;
namespace property_lease_saas.Models.Repositories;
public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _db;
    public PropertyRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Property property)
    {
        _db.Properties.Add(property);
        await SaveAsync();
    }

    public async Task<Property> GetByIdAsync(Guid id)
    {
        return await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var property = await _db.Properties.FindAsync(id);
        if (property != null)
        {
            _db.Properties.Remove(property);
            await SaveAsync();
        }
    }


    public async Task<IEnumerable<Property>> GetByLandlordAsync(string landlordId)
    {
        return await _db.Properties
            .Where(p => p.LandlordId == landlordId)
            .Include(p => p.Images)
            .ToListAsync();
    }

    public async Task<IEnumerable<Property>> GetAvailableAsync()
    {
        return await _db.Properties
            .Include(p=> p.Images)
            .Where(p => p.IsPublished )
            .Where(p=> !p.IsTaken)
            .ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}
