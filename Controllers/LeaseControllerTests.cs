// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Moq;
// using property_lease_saas.Controllers;
// using property_lease_saas.Services;
// using property_lease_saas.Models.Entities;
// using System.Security.Claims;

// namespace property_lease_saas.Tests.Controllers
// {
//     public class LeaseControllerTests
//     {
//         private readonly Mock<LeaseService> _mockLeaseService;
//         private readonly LeaseController _controller;
//         private readonly ClaimsPrincipal _userClaims;

//         public LeaseControllerTests()
//         {
//             _mockLeaseService = new Mock<LeaseService>(
//                 Mock.Of<property_lease_saas.Data.ApplicationDbContext>(),
//                 Mock.Of<property_lease_saas.Services.Notifications.INotificationService>());

//             _controller = new LeaseController(_mockLeaseService.Object);
            
//             // Setup user claims
//             _userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, "user-123"),
//                 new Claim("UserType", "Tenant")
//             }, "TestAuthentication"));
            
//             _controller.ControllerContext = new ControllerContext
//             {
//                 HttpContext = new DefaultHttpContext { User = _userClaims }
//             };
//         }

//         [Fact]
//         public async Task RequestLease_ValidRequest_RedirectsWithSuccess()
//         {
//             // Arrange
//             var propertyId = Guid.NewGuid();
//             _mockLeaseService
//                 .Setup(x => x.RequestAsync(propertyId, "user-123"))
//                 .Returns(Task.CompletedTask);

//             // Act
//             var result = await _controller.RequestLease(propertyId) as RedirectToActionResult;

//             // Assert
//             result.Should().NotBeNull();
//             result.ActionName.Should().Be("Available");
//             result.ControllerName.Should().Be("Properties");
            
//             // Check TempData
//             _controller.TempData["Success"].Should().Be("Lease request submitted successfully.");
//         }

//         [Fact]
//         public async Task RequestLease_InvalidOperation_RedirectsWithError()
//         {
//             // Arrange
//             var propertyId = Guid.NewGuid();
//             _mockLeaseService
//                 .Setup(x => x.RequestAsync(propertyId, "user-123"))
//                 .ThrowsAsync(new InvalidOperationException("Property not found"));

//             // Act
//             var result = await _controller.RequestLease(propertyId) as RedirectToActionResult;

//             // Assert
//             result.Should().NotBeNull();
//             _controller.TempData["Error"].Should().Be("Property not found");
//         }

//         [Fact]
//         public async Task Approve_AsLandlord_ApprovesAndRedirects()
//         {
//             // Arrange
//             var leaseRequestId = Guid.NewGuid();
//             var landlordClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, "landlord-123"),
//                 new Claim("UserType", "Landlord")
//             }, "TestAuthentication"));
            
//             _controller.ControllerContext.HttpContext.User = landlordClaims;
            
//             _mockLeaseService
//                 .Setup(x => x.ApproveAsync(leaseRequestId))
//                 .Returns(Task.CompletedTask);

//             // Act
//             var result = await _controller.Approve(leaseRequestId) as RedirectToActionResult;

//             // Assert
//             result.Should().NotBeNull();
//             result.ActionName.Should().Be("Requests");
//             _controller.TempData["Success"].Should().Be("Lease request approved successfully.");
//         }

//         [Fact]
//         public async Task Index_AsTenant_ReturnsViewWithLeases()
//         {
//             // Arrange
//             var leases = new List<Lease>
//             {
//                 new Lease { Id = Guid.NewGuid(), TenantId = "user-123", Status = LeaseStatus.Active }
//             };
            
//             _mockLeaseService
//                 .Setup(x => x.GetTenantLeasesAsync("user-123"))
//                 .ReturnsAsync(leases);

//             // Act
//             var result = await _controller.Index() as ViewResult;

//             // Assert
//             result.Should().NotBeNull();
//             result.Model.Should().BeEquivalentTo(leases);
//         }

//         [Fact]
//         public async Task Requests_AsLandlord_ReturnsViewWithRequests()
//         {
//             // Arrange
//             var landlordClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, "landlord-123"),
//                 new Claim("UserType", "Landlord")
//             }, "TestAuthentication"));
            
//             _controller.ControllerContext.HttpContext.User = landlordClaims;
            
//             var requests = new List<LeaseRequest>
//             {
//                 new LeaseRequest { Id = Guid.NewGuid(), LandlordId = "landlord-123", Status = LeaseRequestStatus.Pending }
//             };
            
//             _mockLeaseService
//                 .Setup(x => x.GetLandlordLeaseRequestsAsync("landlord-123"))
//                 .ReturnsAsync(requests);

//             // Act
//             var result = await _controller.Requests() as ViewResult;

//             // Assert
//             result.Should().NotBeNull();
//             result.Model.Should().BeEquivalentTo(requests);
//         }

//         [Fact]
//         public async Task GetRequestsPartial_ReturnsPartialView()
//         {
//             // Arrange
//             var landlordClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, "landlord-123"),
//                 new Claim("UserType", "Landlord")
//             }, "TestAuthentication"));
            
//             _controller.ControllerContext.HttpContext.User = landlordClaims;
            
//             var requests = new List<LeaseRequest>
//             {
//                 new LeaseRequest { Id = Guid.NewGuid(), LandlordId = "landlord-123" }
//             };
            
//             _mockLeaseService
//                 .Setup(x => x.GetLandlordLeaseRequestsAsync("landlord-123"))
//                 .ReturnsAsync(requests);

//             // Act
//             var result = await _controller.GetRequestsPartial() as PartialViewResult;

//             // Assert
//             result.Should().NotBeNull();
//             result.ViewName.Should().Be("_RequestsPartial");
//             result.Model.Should().BeEquivalentTo(requests);
//         }
//     }
// }