using property_lease_saas.Infrastructure.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using property_lease_saas.Services;

namespace property_lease_saas.Controllers;

[Authorize(Policy = "MechanicOnly")]
public class MechanicMaintenanceController : Controller
{
    private readonly MaintenanceService _service;
    private readonly IMechanicApplicationService _applicationService;

    public MechanicMaintenanceController(
        MaintenanceService service,
        IMechanicApplicationService applicationService)
    {
        _service = service;
        _applicationService = applicationService;
    }

    // NEW: Applications Dashboard
    public async Task<IActionResult> Applications()
    {
        var mechanicId = User.UserId();
        var data = await _applicationService.GetMechanicApplicationsDataAsync(mechanicId);

        foreach (var item in data)
        {
            ViewData[item.Key] = item.Value;
        }

        ViewBag.MechanicName = User.Identity?.Name ?? "Mechanic";

        return View();
    }

    [Authorize(Policy = "MechanicOnly")]
    public async Task<IActionResult> MyJobs()
    {
        var jobs = await _service.ForMechanicAsync(User.UserId());
        return View(jobs);
    }

    public async Task<IActionResult> Available()
    {
        var jobs = await _service.PublishedAsync();
        return View(jobs);
    }

    [Authorize(Policy = "MechanicOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCompletion(Guid requestId, IFormFile receipt, IFormFile image)
    {
        try
        {
            await _service.UploadCompletionAsync(requestId, receipt, image);
            TempData["Success"] = "Completion evidence uploaded successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(Guid requestId, decimal bill, string notes)
    {
        try
        {
            if (bill <= 0)
            {
                TempData["Error"] = "Bill amount must be greater than zero.";
                return RedirectToAction(nameof(Applications));
            }

            if (string.IsNullOrWhiteSpace(notes) || notes.Length < 10)
            {
                TempData["Error"] = "Notes must be at least 10 characters.";
                return RedirectToAction(nameof(Applications));
            }

            await _service.ApplyAsync(requestId, User.UserId(), bill, notes);
            TempData["Success"] = "Application submitted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid requestId)
    {
        try
        {
            await _service.StartWorkAsync(requestId);
            TempData["Success"] = "Work started successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [Authorize(Policy = "MechanicOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        Guid requestId,
        IFormFile receipt,
        IFormFile completionImage)
    {
        try
        {
            if (receipt == null || completionImage == null)
            {
                TempData["Error"] = "Both receipt and completion image are required.";
                return RedirectToAction(nameof(Applications));
            }

            await _service.CompleteAsync(requestId, receipt, completionImage);
            TempData["Success"] = "Job marked as completed!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Applications));
    }
}