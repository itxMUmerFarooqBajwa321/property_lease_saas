using property_lease_saas.Infrastructure.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;
using property_lease_saas.Models.Repositories;

namespace property_lease_saas.Controllers;

[Authorize(Policy = "LandlordOnly")]
public class LandlordMaintenanceController : Controller
{
    private readonly MaintenanceService _service;
    private readonly IPropertyRepository _propertyRepository;

    public LandlordMaintenanceController(
        MaintenanceService service,
        IPropertyRepository propertyRepository)
    {
        _service = service;
        _propertyRepository = propertyRepository;
    }

    public async Task<IActionResult> Requests()
    {
        var landlordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var data = await _service.ForLandlordAsync(landlordId);
        
        // Pass landlord's properties to ViewBag for the form
        ViewBag.Properties = await _propertyRepository.GetByLandlordAsync(landlordId);
        
        return View(data);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(string title, string description, Guid propertyId)
    {
        await _service.RequestAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), 
            title, 
            description, 
            propertyId);
        return RedirectToAction(nameof(Requests));
    }
    
    [HttpPost]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _service.PublishAsync(id);
        return RedirectToAction(nameof(Requests));
    }
    
    [HttpPost]
    public async Task<IActionResult> Accept(Guid applicationId)
    {
        await _service.AcceptMechanicAsync(applicationId);
        return RedirectToAction(nameof(Requests));
    }
    
    [HttpPost]
    public async Task<IActionResult> Verify(Guid id)
    {
        await _service.VerifyAsync(id);
        return RedirectToAction(nameof(Requests));
    }
}