// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using property_lease_saas.Data;
// using property_lease_saas.Models.Entities;
// using property_lease_saas.Services;
// using property_lease_saas.Services.Notifications;

// namespace property_lease_saas.Tests.Services
// {
//     public class LeaseServiceTests : TestBase
//     {
//         private Mock<INotificationService> _mockNotificationService;
//         private LeaseService _leaseService;
//         private ApplicationDbContext _dbContext;

//         public LeaseServiceTests()
//         {
//             _mockNotificationService = new Mock<INotificationService>();
//             _dbContext = CreateDbContext();
//             _leaseService = new LeaseService(_dbContext, _mockNotificationService.Object);
//         }

//         [Fact]
//         public async Task RequestAsync_ValidRequest_CreatesLeaseRequest()
//         {
//             // Arrange
//             var propertyId = Guid.NewGuid();
//             var tenantId = "tenant-123";
//             var landlordId = "landlord-456";
            
//             var property = new Property
//             {
//                 Id = propertyId,
//                 Title = "Test Property",
//                 LandlordId = landlordId,
//                 Rent = 1000,
//                 IsTaken = false
//             };
            
//             _dbContext.Properties.Add(property);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             await _leaseService.RequestAsync(propertyId, tenantId);

//             // Assert
//             var savedRequest = await _dbContext.LeaseRequests
//                 .FirstOrDefaultAsync(r => r.PropertyId == propertyId && r.TenantId == tenantId);
            
//             savedRequest.Should().NotBeNull();
//             savedRequest.Status.Should().Be(LeaseRequestStatus.Pending);
//             savedRequest.LandlordId.Should().Be(landlordId);
            
//             // Verify notification was sent
//             _mockNotificationService.Verify(
//                 x => x.NotifyLeaseRequestCreated(
//                     propertyId, 
//                     It.IsAny<Guid>(), 
//                     tenantId, 
//                     It.IsAny<string>()),
//                 Times.Once);
//         }

//         [Fact]
//         public async Task RequestAsync_PropertyAlreadyTaken_ThrowsException()
//         {
//             // Arrange
//             var propertyId = Guid.NewGuid();
//             var tenantId = "tenant-123";
            
//             var property = new Property
//             {
//                 Id = propertyId,
//                 Title = "Test Property",
//                 LandlordId = "landlord-456",
//                 Rent = 1000,
//                 IsTaken = true  // Property is already taken
//             };
            
//             _dbContext.Properties.Add(property);
//             await _dbContext.SaveChangesAsync();

//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidOperationException>(
//                 () => _leaseService.RequestAsync(propertyId, tenantId));
//         }

//         [Fact]
//         public async Task ApproveAsync_ValidRequest_ApprovesAndCreatesLease()
//         {
//             // Arrange
//             var leaseRequestId = Guid.NewGuid();
//             var propertyId = Guid.NewGuid();
//             var tenantId = "tenant-123";
//             var landlordId = "landlord-456";
            
//             var property = new Property
//             {
//                 Id = propertyId,
//                 Title = "Test Property",
//                 LandlordId = landlordId,
//                 Rent = 1500,
//                 IsTaken = false
//             };
            
//             var leaseRequest = new LeaseRequest
//             {
//                 Id = leaseRequestId,
//                 PropertyId = propertyId,
//                 TenantId = tenantId,
//                 LandlordId = landlordId,
//                 Status = LeaseRequestStatus.Pending,
//                 RequestedAt = DateTime.UtcNow
//             };
            
//             _dbContext.Properties.Add(property);
//             _dbContext.LeaseRequests.Add(leaseRequest);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             await _leaseService.ApproveAsync(leaseRequestId);

//             // Assert
//             var updatedRequest = await _dbContext.LeaseRequests
//                 .FirstOrDefaultAsync(r => r.Id == leaseRequestId);
            
//             updatedRequest.Status.Should().Be(LeaseRequestStatus.Approved);
            
//             var createdLease = await _dbContext.Leases
//                 .FirstOrDefaultAsync(l => l.LeaseRequestId == leaseRequestId);
            
//             createdLease.Should().NotBeNull();
//             createdLease.RentAmount.Should().Be(1500);
//             createdLease.Status.Should().Be(LeaseStatus.Active);
            
//             var updatedProperty = await _dbContext.Properties
//                 .FirstOrDefaultAsync(p => p.Id == propertyId);
            
//             updatedProperty.IsTaken.Should().BeTrue();
            
//             // Verify notification was sent
//             _mockNotificationService.Verify(
//                 x => x.NotifyLeaseRequestApproved(
//                     leaseRequestId, 
//                     landlordId, 
//                     It.IsAny<string>()),
//                 Times.Once);
//         }

//         [Fact]
//         public async Task ApproveAsync_NonExistentRequest_ThrowsException()
//         {
//             // Arrange
//             var nonExistentId = Guid.NewGuid();

//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidOperationException>(
//                 () => _leaseService.ApproveAsync(nonExistentId));
//         }

//         [Fact]
//         public async Task RejectAsync_ValidRequest_RejectsRequest()
//         {
//             // Arrange
//             var leaseRequestId = Guid.NewGuid();
//             var propertyId = Guid.NewGuid();
//             var tenantId = "tenant-123";
//             var landlordId = "landlord-456";
            
//             var property = new Property
//             {
//                 Id = propertyId,
//                 Title = "Test Property",
//                 LandlordId = landlordId,
//                 Rent = 1000,
//                 IsTaken = false
//             };
            
//             var leaseRequest = new LeaseRequest
//             {
//                 Id = leaseRequestId,
//                 PropertyId = propertyId,
//                 TenantId = tenantId,
//                 LandlordId = landlordId,
//                 Status = LeaseRequestStatus.Pending,
//                 RequestedAt = DateTime.UtcNow
//             };
            
//             _dbContext.Properties.Add(property);
//             _dbContext.LeaseRequests.Add(leaseRequest);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             await _leaseService.RejectAsync(leaseRequestId);

//             // Assert
//             var updatedRequest = await _dbContext.LeaseRequests
//                 .FirstOrDefaultAsync(r => r.Id == leaseRequestId);
            
//             updatedRequest.Status.Should().Be(LeaseRequestStatus.Rejected);
            
//             // Verify notification was sent
//             _mockNotificationService.Verify(
//                 x => x.NotifyLeaseRequestRejected(
//                     leaseRequestId, 
//                     landlordId, 
//                     It.IsAny<string>()),
//                 Times.Once);
//         }

//         [Fact]
//         public async Task GetTenantLeasesAsync_ReturnsOnlyTenantsLeases()
//         {
//             // Arrange
//             var tenantId = "tenant-123";
//             var otherTenantId = "tenant-456";
            
//             var leases = new List<Lease>
//             {
//                 new Lease { Id = Guid.NewGuid(), TenantId = tenantId, Status = LeaseStatus.Active },
//                 new Lease { Id = Guid.NewGuid(), TenantId = tenantId, Status = LeaseStatus.Active },
//                 new Lease { Id = Guid.NewGuid(), TenantId = otherTenantId, Status = LeaseStatus.Active }
//             };
            
//             _dbContext.Leases.AddRange(leases);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             var result = await _leaseService.GetTenantLeasesAsync(tenantId);

//             // Assert
//             result.Should().HaveCount(2);
//             result.All(l => l.TenantId == tenantId).Should().BeTrue();
//         }

//         [Fact]
//         public async Task GetLandlordLeaseRequestsAsync_ReturnsOnlyLandlordsRequests()
//         {
//             // Arrange
//             var landlordId = "landlord-123";
//             var otherLandlordId = "landlord-456";
            
//             var requests = new List<LeaseRequest>
//             {
//                 new LeaseRequest { Id = Guid.NewGuid(), LandlordId = landlordId, Status = LeaseRequestStatus.Pending },
//                 new LeaseRequest { Id = Guid.NewGuid(), LandlordId = landlordId, Status = LeaseRequestStatus.Pending },
//                 new LeaseRequest { Id = Guid.NewGuid(), LandlordId = otherLandlordId, Status = LeaseRequestStatus.Pending }
//             };
            
//             _dbContext.LeaseRequests.AddRange(requests);
//             await _dbContext.SaveChangesAsync();

//             // Act
//             var result = await _leaseService.GetLandlordLeaseRequestsAsync(landlordId);

//             // Assert
//             result.Should().HaveCount(2);
//             result.All(r => r.LandlordId == landlordId).Should().BeTrue();
//         }

//         public override void Dispose()
//         {
//             _dbContext?.Dispose();
//             base.Dispose();
//         }
//     }
// }