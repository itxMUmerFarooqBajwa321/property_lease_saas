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

    public MechanicMaintenanceController(MaintenanceService service)
    {
        _service = service;
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
    public async Task<IActionResult> UploadCompletion(Guid requestId,IFormFile receipt,IFormFile image)
    {
        await _service.UploadCompletionAsync(requestId, receipt, image);
        return RedirectToAction(nameof(MyJobs));
    }

    [HttpPost]
    public async Task<IActionResult> Apply(Guid requestId, decimal bill, string notes)
    {
        await _service.ApplyAsync(requestId, User.FindFirstValue(ClaimTypes.NameIdentifier), bill, notes);
        return RedirectToAction(nameof(Available));
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid requestId)
    {
        await _service.StartWorkAsync(requestId);
        return RedirectToAction(nameof(MyJobs));
    }

[HttpPost]
[Authorize(Policy = "MechanicOnly")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Complete(
    Guid requestId,
    IFormFile receipt,
    IFormFile completionImage)
{
    await _service.CompleteAsync(requestId, receipt, completionImage);
    return RedirectToAction(nameof(MyJobs));
}

}
