using property_lease_saas.Infrastructure.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Models.Entities;
using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Controllers;

[Authorize(Policy = "LandlordOnly")]
public class LandlordMaintenanceController : Controller
{
    private readonly MaintenanceService _service;
    private readonly IPropertyRepository _propertyRepository;
    private readonly ApplicationDbContext _context;

    public LandlordMaintenanceController(
        MaintenanceService service,
        IPropertyRepository propertyRepository,
        ApplicationDbContext context)
    {
        _service = service;
        _propertyRepository = propertyRepository;
        _context = context;
    }

    public async Task<IActionResult> Requests()
    {
        var landlordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var data = await _service.GetAppRepo().ForLandlord(landlordId);
        ViewBag.MaintenanceRequests = await _service.ForLandlordAsync(landlordId);
        ViewBag.Properties = await _propertyRepository.GetByLandlordAsync(landlordId);
        return View(data);
    }

    public async Task<IActionResult> Applications()
    {
        var landlordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requests = await _service.ForLandlordAsync(landlordId);
        ViewBag.Properties = await _propertyRepository.GetByLandlordAsync(landlordId);
        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string description, Guid propertyId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length < 5)
            {
                TempData["Error"] = "Title must be at least 5 characters long.";
                return RedirectToAction(nameof(Applications));
            }

            if (string.IsNullOrWhiteSpace(description) || description.Length < 10)
            {
                TempData["Error"] = "Description must be at least 10 characters long.";
                return RedirectToAction(nameof(Applications));
            }

            var landlordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            await _service.RequestAsync(landlordId, title, description, propertyId);
            
            TempData["Success"] = "Maintenance request created successfully!";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating maintenance request: {ex.Message}");
            TempData["Error"] = ex.Message;
        }
        
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            await _service.PublishAsync(id);
            TempData["Success"] = "Maintenance request published to mechanics!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid applicationId)
    {
        try
        {
            await _service.AcceptMechanicAsync(applicationId);
            TempData["Success"] = "Mechanic application accepted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(Guid id)
    {
        try
        {
            await _service.VerifyAsync(id);
            TempData["Success"] = "Maintenance work verified successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        
        return RedirectToAction(nameof(Applications));
    }

    // NEW: Get Request Detail (for modal)
    [HttpGet]
    public async Task<IActionResult> GetRequestDetail(Guid id)
    {
        var request = await _context.MaintenanceRequests
            .Include(r => r.Applications)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return PartialView("_RequestDetailPartial", null);
        }

        // Fetch related data
        var property = await _context.Properties.FindAsync(request.PropertyId);
        var landlord = await _context.Users.FindAsync(request.LandlordId);
        var tenant = !string.IsNullOrEmpty(request.TenantId) 
            ? await _context.Users.FindAsync(request.TenantId) 
            : null;
        var mechanic = !string.IsNullOrEmpty(request.AssignedMechanicId) 
            ? await _context.Users.FindAsync(request.AssignedMechanicId) 
            : null;

        ViewBag.Property = property;
        ViewBag.Landlord = landlord;
        ViewBag.Tenant = tenant;
        ViewBag.Mechanic = mechanic;

        return PartialView("_RequestDetailPartial", request);
    }

    // NEW: Get Application Detail (for modal)
    [HttpGet]
    public async Task<IActionResult> GetApplicationDetail(Guid applicationId, Guid requestId)
    {
        var application = await _context.MaintenanceApplications
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (application == null || request == null)
        {
            return PartialView("_ApplicationDetailPartial", null);
        }

        var mechanic = await _context.Users.FindAsync(application.MechanicId);
        
        ViewBag.Request = request;
        ViewBag.Mechanic = mechanic;

        return PartialView("_ApplicationDetailPartial", application);
    }
}