using property_lease_saas.Data;
using property_lease_saas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Services; // Add this
// using property_lease_saas.Hubs;
public class LeaseService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService; // Add this
    private readonly IHttpContextAccessor _httpContextAccessor; // Optional: for getting current user info

    public LeaseService(
        ApplicationDbContext context, 
        INotificationService notificationService, // Add this
        IHttpContextAccessor httpContextAccessor = null) // Optional
    {
        _context = context;
        _notificationService = notificationService; // Add this
        _httpContextAccessor = httpContextAccessor;
    }

    // Method to request lease (from your controller)
    public async Task RequestAsync(Guid propertyId, string tenantId)
    {
        Console.WriteLine("Enter in LeaseService::RequestAsync()");
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId);
        
        if (property == null)
            throw new InvalidOperationException("Property not found.");
        
        if (property.IsTaken)
            throw new InvalidOperationException("Property is already taken.");



        var leaseRequest = new LeaseRequest
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            TenantId = tenantId,
            LandlordId = property.LandlordId,
            Status = LeaseRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.LeaseRequests.Add(leaseRequest);
        await _context.SaveChangesAsync();

        // Send notification to landlord
        var tenant = await _context.Users.FirstOrDefaultAsync(u => u.Id == tenantId);
        var tenantName = tenant?.FullName ?? "A tenant";
        
        await _notificationService.NotifyLeaseRequestCreated(
            propertyId, 
            leaseRequest.Id, 
            tenantId, 
            tenantName);
        Console.WriteLine("Exit from LeaseService::RequestAsync()");
        
    }

    // Method to approve lease
    public async Task ApproveAsync(Guid leaseRequestId)
    {
        var leaseRequest = await _context.LeaseRequests
            .Include(lr => lr.Property)
            .FirstOrDefaultAsync(lr => lr.Id == leaseRequestId);
        
        if (leaseRequest == null)
            throw new InvalidOperationException("Lease request not found.");
        
        // Update status
        leaseRequest.Status = LeaseRequestStatus.Approved;
        leaseRequest.Property.IsTaken = true; // Mark property as taken
        
        // Create actual Lease record (if you have one)
        var lease = new Lease // You might need to create this model
        {
            Id = Guid.NewGuid(),
            PropertyId = leaseRequest.PropertyId,
            TenantId = leaseRequest.TenantId,
            LandlordId = leaseRequest.LandlordId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1), // Example: 1 year lease
            RentAmount = leaseRequest.Property.Rent,
            Status = LeaseStatus.Active // If you have LeaseStatus enum
        };
        
        _context.Leases.Add(lease);
        await _context.SaveChangesAsync();

        // Send notification to tenant
        var landlord = await _context.Users.FirstOrDefaultAsync(u => u.Id == leaseRequest.LandlordId);
        var landlordName = landlord?.FullName ?? "The landlord";
        
        await _notificationService.NotifyLeaseRequestApproved(
            leaseRequestId, 
            leaseRequest.LandlordId, 
            landlordName);
    }

    // Method to reject lease
    public async Task RejectAsync(Guid leaseRequestId)
    {
        var leaseRequest = await _context.LeaseRequests
            .Include(lr => lr.Property)
            .FirstOrDefaultAsync(lr => lr.Id == leaseRequestId);
        
        if (leaseRequest == null)
            throw new InvalidOperationException("Lease request not found.");
        
        leaseRequest.Status = LeaseRequestStatus.Rejected;
        await _context.SaveChangesAsync();

        // Send notification to tenant
        var landlord = await _context.Users.FirstOrDefaultAsync(u => u.Id == leaseRequest.LandlordId);
        var landlordName = landlord?.FullName ?? "The landlord";
        
        await _notificationService.NotifyLeaseRequestRejected(
            leaseRequestId, 
            leaseRequest.LandlordId, 
            landlordName);
    }

    // Your existing methods...
    public async Task<List<Lease>> ForTenantAsync(string tenantId)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .Where(l => l.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<List<LeaseRequest>> ForLandlordAsync(string landlordId)
    {
        return await _context.LeaseRequests
            .Include(lr => lr.Property)
            .Where(lr => lr.LandlordId == landlordId)
            .ToListAsync();
    }

    public async Task<List<LeaseRequest>> GetTenantLeaseRequestsAsync(string tenantId)
    {
        return await _context.LeaseRequests
            .Include(lr => lr.Property)
            .Where(lr => lr.TenantId == tenantId)
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();
    }

    // Get tenant's active leases
    public async Task<List<Lease>> GetTenantLeasesAsync(string tenantId)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.LeaseRequest)
            .Where(l => l.TenantId == tenantId && l.Status == LeaseStatus.Active)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }

    // Get landlord's lease requests
    public async Task<List<LeaseRequest>> GetLandlordLeaseRequestsAsync(string landlordId)
    {
        return await _context.LeaseRequests
            .Include(lr => lr.Property)
            .Include(lr => lr.Lease)
            .Where(lr => lr.LandlordId == landlordId)
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();
    }

    // Get landlord's active leases
    public async Task<List<Lease>> GetLandlordLeasesAsync(string landlordId)
    {
        return await _context.Leases
            .Include(l => l.Property)
            .Where(l => l.LandlordId == landlordId && l.Status == LeaseStatus.Active)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }

    
}