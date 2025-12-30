using property_lease_saas.Infrastructure.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;

namespace property_lease_saas.Controllers;

[Authorize(Policy = "TenantOnly")]
public class TenantMaintenanceController : Controller
{
    private readonly MaintenanceService _service;
    
    public TenantMaintenanceController(MaintenanceService service)
    {
        _service = service;
    }
    
    public IActionResult Create(Guid leaseId, Guid propertyId, string landlordId)
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Guid leaseId, 
        Guid propertyId, 
        string landlordId, 
        string title, 
        string description)
    {
        // Use CreateRequestAsync which takes 6 parameters
        await _service.CreateRequestAsync(
            leaseId, 
            propertyId, 
            User.FindFirstValue(ClaimTypes.NameIdentifier)!, 
            landlordId, 
            title, 
            description);
        
        return RedirectToAction("MyRequests");
    }
}