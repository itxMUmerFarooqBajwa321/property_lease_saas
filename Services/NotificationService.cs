using Microsoft.AspNetCore.SignalR;
using property_lease_saas.Hubs;
using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Models.Entities;
using property_lease_saas.Models;

namespace property_lease_saas.Services
{

    public interface INotificationService
    {
        // Existing lease methods
        Task NotifyLeaseRequestCreated(Guid propertyId, Guid leaseRequestId, string tenantId, string tenantName);
        Task NotifyLeaseRequestApproved(Guid leaseRequestId, string landlordId, string landlordName);
        Task NotifyLeaseRequestRejected(Guid leaseRequestId, string landlordId, string landlordName);

        // NEW: Maintenance notification methods
        Task NotifyMaintenanceRequestCreated(Guid requestId, string tenantId, string tenantName, string propertyTitle, string landlordId);
        Task NotifyMaintenanceRequestPublished(Guid requestId);
        Task NotifyMaintenanceApplicationReceived(Guid applicationId, Guid requestId, string mechanicId, string mechanicName, decimal proposedBill);
        Task NotifyMaintenanceApplicationAccepted(Guid requestId, string mechanicId, string mechanicName);
        Task NotifyMaintenanceWorkStarted(Guid requestId);
        Task NotifyMaintenanceWorkCompleted(Guid requestId);
        Task NotifyMaintenanceWorkVerified(Guid requestId);
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
                    type = "LeaseRequestCreated",
                    title = "New Lease Request",
                    message = $"{tenantName} has requested to lease your property '{property.Title}'",
                    leaseRequestId = leaseRequestId,
                    propertyId = propertyId,
                    tenantId = tenantId,
                    timestamp = DateTime.UtcNow
                });

            // Also notify all connections for this specific property
            await _hubContext.Clients.Group($"property-{propertyId}")
                .SendAsync("ReceivePropertyNotification", new
                {
                    type = "LeaseRequestCreated",
                    propertyId = propertyId,
                    message = "A new lease request has been submitted",
                    timestamp = DateTime.UtcNow
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
                    type = "LeaseRequestApproved",
                    title = "Lease Request Approved!",
                    message = $"Your lease request for '{leaseRequest.Property.Title}' has been approved by {landlordName}",
                    leaseRequestId = leaseRequestId,
                    propertyId = leaseRequest.PropertyId,
                    landlordId = landlordId,
                    timestamp = DateTime.UtcNow
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
                    type = "LeaseRequestRejected",
                    title = "Lease Request Rejected",
                    message = $"Your lease request for '{leaseRequest.Property.Title}' has been rejected by {landlordName}",
                    leaseRequestId = leaseRequestId,
                    propertyId = leaseRequest.PropertyId,
                    landlordId = landlordId,
                    timestamp = DateTime.UtcNow
                });
        }


        // 1️⃣ TENANT CREATES MAINTENANCE REQUEST
        public async Task NotifyMaintenanceRequestCreated(
            Guid requestId,
            string tenantId,
            string tenantName,
            string propertyTitle,
            string landlordId)
        {
            Console.WriteLine($"DEBUG: Sending maintenance request notification to landlord: {landlordId}");

            // Send to landlord
            await _hubContext.Clients.Group($"user-{landlordId}")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceRequestCreated",
                    title = "New Maintenance Request",
                    message = $"{tenantName} has requested maintenance for {propertyTitle}",
                    requestId = requestId,
                    tenantId = tenantId,
                    timestamp = DateTime.UtcNow
                });
        }

        // 2️⃣ LANDLORD PUBLISHES REQUEST (makes visible to mechanics)

        public async Task NotifyMaintenanceRequestPublished(Guid requestId)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            // Manually fetch property
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId);
            var propertyTitle = property?.Title ?? "a property";

            Console.WriteLine($"DEBUG: Publishing maintenance request to all mechanics");

            // Notify ALL mechanics
            await _hubContext.Clients.Group("all-mechanics")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceRequestPublished",
                    title = "New Maintenance Job Available",
                    message = $"New job: {request.Title} at {propertyTitle}",
                    requestId = requestId,
                    propertyTitle = propertyTitle,
                    timestamp = DateTime.UtcNow
                });

            // Notify tenant (if exists)
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                await _hubContext.Clients.Group($"user-{request.TenantId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceRequestPublished",
                        title = "Maintenance Request Published",
                        message = $"Your maintenance request has been published. Mechanics can now apply.",
                        requestId = requestId,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        // 3️⃣ MECHANIC APPLIES FOR JOB
        public async Task NotifyMaintenanceApplicationReceived(
    Guid applicationId,
    Guid requestId,
    string mechanicId,
    string mechanicName,
    decimal proposedBill)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            Console.WriteLine($"DEBUG: Notifying landlord about mechanic application from {mechanicName}");

            // Notify landlord
            await _hubContext.Clients.Group($"user-{request.LandlordId}")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceApplicationReceived",
                    title = "New Mechanic Application",
                    message = $"{mechanicName} applied for '{request.Title}' with bid of Rs. {proposedBill:N0}",
                    applicationId = applicationId,
                    requestId = requestId,
                    mechanicId = mechanicId,
                    mechanicName = mechanicName,
                    proposedBill = proposedBill,
                    timestamp = DateTime.UtcNow
                });
        }


        // 4️⃣ LANDLORD ACCEPTS MECHANIC
        public async Task NotifyMaintenanceApplicationAccepted(
    Guid requestId,
    string mechanicId,
    string mechanicName)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            // Manually fetch property
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId);
            var propertyTitle = property?.Title ?? "a property";

            Console.WriteLine($"DEBUG: Notifying mechanic and tenant about accepted application");

            // Notify mechanic
            await _hubContext.Clients.Group($"user-{mechanicId}")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceApplicationAccepted",
                    title = "Application Accepted!",
                    message = $"Your application for '{request.Title}' has been accepted",
                    requestId = requestId,
                    propertyTitle = propertyTitle,
                    timestamp = DateTime.UtcNow
                });

            // Notify tenant (if exists)
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                await _hubContext.Clients.Group($"user-{request.TenantId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceApplicationAccepted",
                        title = "Mechanic Assigned",
                        message = $"{mechanicName} has been assigned to your maintenance request",
                        requestId = requestId,
                        mechanicName = mechanicName,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        // 5️⃣ MECHANIC STARTS WORK
        public async Task NotifyMaintenanceWorkStarted(Guid requestId)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            // Manually fetch mechanic
            var mechanic = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.AssignedMechanicId);
            var mechanicName = mechanic?.FullName ?? "Mechanic";

            Console.WriteLine($"DEBUG: Notifying landlord and tenant about work started");

            // Notify landlord
            await _hubContext.Clients.Group($"user-{request.LandlordId}")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceWorkStarted",
                    title = "Maintenance Work Started",
                    message = $"{mechanicName} has started work on '{request.Title}'",
                    requestId = requestId,
                    timestamp = DateTime.UtcNow
                });

            // Notify tenant (if exists)
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                await _hubContext.Clients.Group($"user-{request.TenantId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceWorkStarted",
                        title = "Work Started",
                        message = $"Maintenance work has started on your request",
                        requestId = requestId,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        // 6️⃣ MECHANIC COMPLETES WORK
        public async Task NotifyMaintenanceWorkCompleted(Guid requestId)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            // Manually fetch mechanic
            var mechanic = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.AssignedMechanicId);
            var mechanicName = mechanic?.FullName ?? "Mechanic";

            Console.WriteLine($"DEBUG: Notifying landlord and tenant about work completion");

            // Notify landlord
            await _hubContext.Clients.Group($"user-{request.LandlordId}")
                .SendAsync("ReceiveNotification", new
                {
                    type = "MaintenanceWorkCompleted",
                    title = "Maintenance Work Completed",
                    message = $"{mechanicName} completed work on '{request.Title}' - needs verification",
                    requestId = requestId,
                    timestamp = DateTime.UtcNow
                });

            // Notify tenant (if exists)
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                await _hubContext.Clients.Group($"user-{request.TenantId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceWorkCompleted",
                        title = "Work Completed",
                        message = $"Your maintenance request has been completed",
                        requestId = requestId,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        // 7️⃣ LANDLORD VERIFIES COMPLETION
        // 7️⃣ LANDLORD VERIFIES COMPLETION
        public async Task NotifyMaintenanceWorkVerified(Guid requestId)
        {
            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            Console.WriteLine($"DEBUG: Notifying mechanic and tenant about verification");

            // Notify mechanic
            if (!string.IsNullOrEmpty(request.AssignedMechanicId))
            {
                await _hubContext.Clients.Group($"user-{request.AssignedMechanicId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceWorkVerified",
                        title = "Work Verified",
                        message = $"Your work on '{request.Title}' has been verified",
                        requestId = requestId,
                        timestamp = DateTime.UtcNow
                    });
            }

            // Notify tenant (if exists)
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                await _hubContext.Clients.Group($"user-{request.TenantId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        type = "MaintenanceWorkVerified",
                        title = "Maintenance Request Closed",
                        message = $"Your maintenance request has been verified and closed",
                        requestId = requestId,
                        timestamp = DateTime.UtcNow
                    });
            }
        }
    }
}
