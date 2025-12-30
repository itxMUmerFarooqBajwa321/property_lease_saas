using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace property_lease_saas.Controllers
{
    [Authorize]
    public class PaymentsController:Controller
    {
        public IActionResult Index()
        {
            return View();   
        }

        public IActionResult Details()
        {
            return View();
        }
    }
}