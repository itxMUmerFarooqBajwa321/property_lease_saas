using property_lease_saas.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Services;

namespace property_lease_saas.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var userType = User.FindFirst("UserType")?.Value;
            var userId = User.UserId();

            ViewBag.UserType = userType;
            ViewBag.UserName = User.Identity?.Name ?? "User";
            ViewBag.UserId = userId;

            Dictionary<string, object> stats;

            switch (userType)
            {
                case "Landlord":
                    stats = await _dashboardService.GetLandlordStatsAsync(userId);
                    foreach (var stat in stats)
                    {
                        ViewBag.GetType().GetProperty(stat.Key)?.SetValue(ViewBag, stat.Value);
                        ViewData[stat.Key] = stat.Value;
                    }
                    return View("Landlord");

                case "Tenant":
                    stats = await _dashboardService.GetTenantStatsAsync(userId);
                    foreach (var stat in stats)
                    {
                        ViewData[stat.Key] = stat.Value;
                    }
                    return View("Tenant");

                case "Mechanic":
                    stats = await _dashboardService.GetMechanicStatsAsync(userId);
                    foreach (var stat in stats)
                    {
                        ViewData[stat.Key] = stat.Value;
                    }
                    return View("Mechanic");

                default:
                    return View("Index");
            }
        }
    }
}