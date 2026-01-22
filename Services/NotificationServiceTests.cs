// using FluentAssertions;
// using Microsoft.AspNetCore.SignalR;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Moq;
// using property_lease_saas.Data;
// using property_lease_saas.Hubs;
// using property_lease_saas.Models.Entities;
// using property_lease_saas.Services.Notifications;

// namespace property_lease_saas.Tests.Services
// {
//     public class NotificationServiceTests : TestBase
//     {
//         private Mock<IHubContext<NotificationHub>> _mockHubContext;
//         private Mock<ILogger<NotificationService>> _mockLogger;
//         private Mock<IClientProxy> _mockClientProxy;
//         private NotificationService _notificationService;
//         private ApplicationDbContext _dbContext;

//         public NotificationServiceTests()
//         {
//             _mockHubContext = new Mock<IHubContext<NotificationHub>>();
//             _mockLogger = new Mock<ILogger<NotificationService>>();
//             _mockClientProxy = new Mock<IClientProxy>();
            
//             _dbContext = CreateDbContext();
            
//             var mockClients = new Mock<IHubClients>();
//             var mockGroup = new Mock<IClientProxy>();
            
//             mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
//             _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            
//             _notificationService = new NotificationService(
//                 _mockHubContext.Object, 
//                 _dbContext,
//                 _mockLogger.Object);
//         }

//         [Fact]
//         public async Task NotifyLeaseRequestCreated_SendsNotificationToLandlord()
//         {
//             // Arrange
//             var propertyId = Guid.NewGuid();
//             var leaseRequestId = Guid.NewGuid();
//             var tenantId = "tenant-123";
//             var tenantName = "John Doe";
//             var landlordId = "landlord-456";
            
//             var property = new Property
//             {
//                 Id = propertyId,
//                 Title = "Test Property",
//                 LandlordId = landlordId
//             };
            
//             _dbContext.Properties.Add(property);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             await _notificationService.NotifyLeaseRequestCreated(
//                 propertyId, leaseRequestId, tenantId, tenantName);

//             // Assert
//             _mockClientProxy.Verify(
//                 x => x.SendCoreAsync(
//                     "ReceiveNotification",
//                     It.Is<object[]>(args => 
//                         args != null && 
//                         ((dynamic)args[0]).Type == "LeaseRequestCreated" &&
//                         ((dynamic)args[0]).LandlordId == landlordId),
//                     default),
//                 Times.Once);
//         }

//         [Fact]
//         public async Task NotifyLeaseRequestApproved_SendsNotificationToTenant()
//         {
//             // Arrange
//             var leaseRequestId = Guid.NewGuid();
//             var landlordId = "landlord-456";
//             var landlordName = "Jane Smith";
//             var tenantId = "tenant-123";
            
//             var property = new Property
//             {
//                 Id = Guid.NewGuid(),
//                 Title = "Test Property"
//             };
            
//             var leaseRequest = new LeaseRequest
//             {
//                 Id = leaseRequestId,
//                 PropertyId = property.Id,
//                 TenantId = tenantId,
//                 LandlordId = landlordId,
//                 Property = property
//             };
            
//             _dbContext.Properties.Add(property);
//             _dbContext.LeaseRequests.Add(leaseRequest);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             await _notificationService.NotifyLeaseRequestApproved(
//                 leaseRequestId, landlordId, landlordName);

//             // Assert
//             _mockClientProxy.Verify(
//                 x => x.SendCoreAsync(
//                     "ReceiveNotification",
//                     It.Is<object[]>(args => 
//                         args != null && 
//                         ((dynamic)args[0]).Type == "LeaseRequestApproved" &&
//                         ((dynamic)args[0]).TenantId == tenantId),
//                     default),
//                 Times.Once);
//         }

//         [Fact]
//         public async Task NotifyLeaseRequestCreated_PropertyNotFound_DoesNotSendNotification()
//         {
//             // Arrange
//             var nonExistentPropertyId = Guid.NewGuid();

//             // Act
//             await _notificationService.NotifyLeaseRequestCreated(
//                 nonExistentPropertyId, Guid.NewGuid(), "tenant-123", "John Doe");

//             // Assert
//             _mockClientProxy.Verify(
//                 x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
//                 Times.Never);
//         }

//         public override void Dispose()
//         {
//             _dbContext?.Dispose();
//             base.Dispose();
//         }
//     }
// }