using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        // Implement your notification marking logic here
        // For now, just return success
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        // Return empty array for now
        // You'll need to implement actual notification storage
        return Json(new List<object>());
    }
}   