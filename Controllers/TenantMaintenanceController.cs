using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Controllers;

[Authorize(Policy = "TenantOnly")]
public class TenantMaintenanceController : Controller
{
    private readonly MaintenanceService _service;
    private readonly LeaseRepository _leaseRepo;
    
    public TenantMaintenanceController(
        MaintenanceService service,
        LeaseRepository leaseRepo)
    {
        _service = service;
        _leaseRepo = leaseRepo;
    }
    
    // Show form with dropdown of tenant's active leases
    public async Task<IActionResult> Create()
    {
        var tenantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // Get all active leases for this tenant
        var leases = await _leaseRepo.GetForTenantAsync(tenantId);
        
        // Filter only active leases
        var activeLeases = leases
            .Where(l => l.Status == LeaseStatus.Active)
            .ToList();
        
        ViewBag.Leases = activeLeases;
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Guid leaseId, 
        string title, 
        string description)
    {
        var tenantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // Get the lease to extract propertyId and landlordId
        var lease = await _leaseRepo.GetByIdAsync(leaseId);
        
        if (lease == null)
        {
            ModelState.AddModelError("", "Invalid lease selected");
            return RedirectToAction("Create");
        }
        
        // Verify this lease belongs to the current tenant
        if (lease.TenantId != tenantId)
        {
            return Forbid();
        }
        
        // Create the maintenance request
        await _service.CreateRequestAsync(
            leaseId, 
            lease.PropertyId, 
            tenantId, 
            lease.LandlordId, 
            title, 
            description);
        
        return RedirectToAction("MyRequests");
    }
    
    // View tenant's maintenance requests
    public async Task<IActionResult> MyRequests()
    {
        var tenantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var requests = await _service.ForTenantAsync(tenantId);
        
        return View(requests);
    }
}