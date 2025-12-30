using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace property_lease_saas.Controllers
{
    [Authorize]
    public class DocumentsController:Controller
    {
        public IActionResult Index()
        {
            return View();   
        }

    }
}