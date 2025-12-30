using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace property_lease_saas.Controllers
{
    public class LogoutController:Controller
    {
        public IActionResult Index()
        {
            return View();   
        }

    }
}