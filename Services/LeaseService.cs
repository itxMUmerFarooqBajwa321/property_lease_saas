using property_lease_saas.Models.Entities;
using property_lease_saas.Models.Repositories;

namespace property_lease_saas.Services;

public class LeaseService
{
    private readonly LeaseRepository _leaseRepo;
    private readonly IPropertyRepository _propertyRepo;

    public LeaseService(LeaseRepository leaseRepo, IPropertyRepository propertyRepo)
    {
        _leaseRepo = leaseRepo;
        _propertyRepo = propertyRepo;
    }

    // Tenant requests lease
    public async Task RequestAsync(Guid propertyId, string tenantId)
    {
        var property = await _propertyRepo.GetByIdAsync(propertyId);

        if (property == null || !property.IsPublished)
            throw new Exception("Property not available");
        
        // if alaready request for same property by the user
        var alreadyRequested = await _leaseRepo.ExistsAsync(propertyId, tenantId);

        if (alreadyRequested)
            throw new InvalidOperationException("You have already requested this property.");


        var lease = new Lease
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            TenantId = tenantId,
            LandlordId = property.LandlordId,
            RentAmount = property.Rent,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(12),
            Status = LeaseStatus.Pending
        };

        await _leaseRepo.AddAsync(lease);
    }

    // Landlord actions
    public async Task ApproveAsync(Guid leaseId)
    {
        var lease = await _leaseRepo.GetByIdAsync(leaseId);
        if (lease == null) return;

        lease.Status = LeaseStatus.Approved;
        lease.Property.IsTaken = true;
        lease.StartDate = DateTime.UtcNow;

        await _leaseRepo.UpdateAsync(lease);
    }

    public async Task RejectAsync(Guid leaseId)
    {
        var lease = await _leaseRepo.GetByIdAsync(leaseId);
        if (lease == null) return;

        lease.Status = LeaseStatus.Rejected;
        await _leaseRepo.UpdateAsync(lease);
    }

    public Task<List<Lease>> ForLandlordAsync(string landlordId)
        => _leaseRepo.GetForLandlordAsync(landlordId);

    public Task<List<Lease>> ForTenantAsync(string tenantId)
        => _leaseRepo.GetForTenantAsync(tenantId);
}
