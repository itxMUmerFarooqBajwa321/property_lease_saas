using property_lease_saas.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Controllers;

[Authorize]
public class LeaseController : Controller
{
    private readonly LeaseService _service;

    public LeaseController(LeaseService service)
    {
        _service = service;
    }

    // POST: Tenant requests lease
    [Authorize(Policy = "TenantOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestLease(Guid propertyId)
    {
        try
        {
            await _service.RequestAsync(propertyId, User.UserId());
            TempData["Success"] = "Lease request submitted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Available", "Properties");
    }

    // ===========================================
    // TENANT ACTIONS
    // ===========================================
    
    // GET: Tenant sees their active LEASES
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> Index()
    {
        // This should return LEASES, not LeaseRequests
        var leases = await _service.GetTenantLeasesAsync(User.UserId());
        return View(leases); // Expects IEnumerable<Lease>
    }

    // GET: Tenant sees their lease REQUESTS (pending/approved/rejected)
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> MyRequests()
    {
        var leaseRequests = await _service.GetTenantLeaseRequestsAsync(User.UserId());
        return View("TenantRequests", leaseRequests); // Expects IEnumerable<LeaseRequest>
    }

    // ===========================================
    // LANDLORD ACTIONS
    // ===========================================
    
    // GET: Landlord sees incoming lease REQUESTS
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> Requests()
    {
        // This should return LeaseRequests
        var leaseRequests = await _service.GetLandlordLeaseRequestsAsync(User.UserId());
        return View(leaseRequests); // Expects IEnumerable<LeaseRequest>
    }

    // GET: Landlord sees approved LEASES
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> MyLeasedProperties()
    {
        // This should return Leases
        var leases = await _service.GetLandlordLeasesAsync(User.UserId());
        return View(leases); // Expects IEnumerable<Lease>
    }

    // ===========================================
    // LANDLORD ACTIONS (Approve/Reject)
    // ===========================================
    
    [Authorize(Policy = "LandlordOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid leaseRequestId)
    {
        try
    {
        await _service.ApproveAsync(leaseRequestId);
        TempData["Success"] = "Lease request approved successfully.";
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"DEBUG: Error approving lease: {ex.Message}");
        TempData["Error"] = ex.Message;
    }
    
    return RedirectToAction(nameof(Requests));
    }

    [Authorize(Policy = "LandlordOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid leaseRequestId)
    {
        await _service.RejectAsync(leaseRequestId);
        TempData["Success"] = "Lease request rejected.";
        return RedirectToAction(nameof(Requests));
    }

    // ===========================================
    // AJAX PARTIAL VIEWS
    // ===========================================
    
    // AJAX endpoint for landlord's requests partial view
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> GetRequestsPartial()
    {
        var leaseRequests = await _service.GetLandlordLeaseRequestsAsync(User.UserId());
        return PartialView("_RequestsPartial", leaseRequests); // Expects IEnumerable<LeaseRequest>
    }

    // AJAX endpoint for tenant's leases partial view  
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> GetTenantLeasesPartial()
    {
        var leases = await _service.GetTenantLeasesAsync(User.UserId());
        return PartialView("_TenantLeasesPartial", leases); // Expects IEnumerable<Lease>
    }
    
    // AJAX endpoint for tenant's lease requests partial view
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> GetTenantRequestsPartial()
    {
        var leaseRequests = await _service.GetTenantLeaseRequestsAsync(User.UserId());
        return PartialView("_TenantRequestsPartial", leaseRequests); // Expects IEnumerable<LeaseRequest>
    }
}