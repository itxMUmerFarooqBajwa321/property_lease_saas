using Microsoft.AspNetCore.SignalR;
using property_lease_saas.Hubs;
using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Services{
    
    public interface INotificationService
    {
        Task NotifyLeaseRequestCreated(Guid propertyId, Guid leaseRequestId, string tenantId, string tenantName);
        Task NotifyLeaseRequestApproved(Guid leaseRequestId, string landlordId, string landlordName);
        Task NotifyLeaseRequestRejected(Guid leaseRequestId, string landlordId, string landlordName);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task NotifyLeaseRequestCreated(Guid propertyId, Guid leaseRequestId, string tenantId, string tenantName)
        {
            // Get landlord ID for this property
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId);
            
            if (property == null) return;

            var landlordId = property.LandlordId;

            // Send notification to landlord
            await _hubContext.Clients.Group($"user-{landlordId}")
                .SendAsync("ReceiveNotification", new
                {
                    Type = "LeaseRequestCreated",
                    Title = "New Lease Request",
                    Message = $"{tenantName} has requested to lease your property '{property.Title}'",
                    LeaseRequestId = leaseRequestId,
                    PropertyId = propertyId,
                    TenantId = tenantId,
                    Timestamp = DateTime.UtcNow
                });

            // Also notify all connections for this specific property
            await _hubContext.Clients.Group($"property-{propertyId}")
                .SendAsync("ReceivePropertyNotification", new
                {
                    Type = "LeaseRequestCreated",
                    PropertyId = propertyId,
                    Message = "A new lease request has been submitted",
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifyLeaseRequestApproved(Guid leaseRequestId, string landlordId, string landlordName)
        {
            // Get lease request details
            var leaseRequest = await _context.LeaseRequests
                .Include(lr => lr.Property)
                .FirstOrDefaultAsync(lr => lr.Id == leaseRequestId);
            
            if (leaseRequest == null) return;

            // Send notification to tenant
            await _hubContext.Clients.Group($"user-{leaseRequest.TenantId}")
                .SendAsync("ReceiveNotification", new
                {
                    Type = "LeaseRequestApproved",
                    Title = "Lease Request Approved!",
                    Message = $"Your lease request for '{leaseRequest.Property.Title}' has been approved by {landlordName}",
                    LeaseRequestId = leaseRequestId,
                    PropertyId = leaseRequest.PropertyId,
                    LandlordId = landlordId,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifyLeaseRequestRejected(Guid leaseRequestId, string landlordId, string landlordName)
        {
            // Get lease request details
            var leaseRequest = await _context.LeaseRequests
                .Include(lr => lr.Property)
                .FirstOrDefaultAsync(lr => lr.Id == leaseRequestId);
            
            if (leaseRequest == null) return;

            // Send notification to tenant
            await _hubContext.Clients.Group($"user-{leaseRequest.TenantId}")
                .SendAsync("ReceiveNotification", new
                {
                    Type = "LeaseRequestRejected",
                    Title = "Lease Request Rejected",
                    Message = $"Your lease request for '{leaseRequest.Property.Title}' has been rejected by {landlordName}",
                    LeaseRequestId = leaseRequestId,
                    PropertyId = leaseRequest.PropertyId,
                    LandlordId = landlordId,
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}