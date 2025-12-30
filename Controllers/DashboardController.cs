using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace property_lease_saas.Controllers
{
    public class DashboardController:Controller
    {
        public IActionResult Index()
        {
            return View();   
        }

    }
}