using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Infrastructure.Extensions;
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

    // GET: Tenant sees all lease requests (Pending / Approved / Rejected)
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> Index()
    {
        var leases = await _service.ForTenantAsync(User.UserId());
        return View(leases);
    }

    /* ============================
       LANDLORD
       ============================ */

    // GET: Landlord sees all incoming requests
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> Requests()
    {
        var leases = await _service.ForLandlordAsync(User.UserId());
        return View(leases);
    }

    // GET: Landlord sees only approved (leased) properties
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> MyLeasedProperties()
    {
        var leases = await _service.ForLandlordAsync(User.UserId());

        var approvedLeases = leases
            .Where(l => l.Status == LeaseStatus.Approved)
            .ToList();

        return View(approvedLeases);
    }

    /* ============================
       LANDLORD ACTIONS
       ============================ */

    [Authorize(Policy = "LandlordOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid leaseId)
    {
        await _service.ApproveAsync(leaseId);
        return RedirectToAction("Available", "Properties");
    }

    [Authorize(Policy = "LandlordOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid leaseId)
    {
        await _service.RejectAsync(leaseId);
        return RedirectToAction(nameof(Requests));
    }
}
