using property_lease_saas.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace property_lease_saas.Controllers
{
    [Authorize]
    public class LogoutController:Controller
    {
        public IActionResult Index()
        {
            return View();   
        }

    }
}