using property_lease_saas.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Add this

namespace property_lease_saas.Controllers
{
    [Authorize(Policy = "LandlordOnly")]
    public class TenantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Add constructor
        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // If you need leased properties, store them or use them
            var leasedProperties = await _context.Properties
                .Where(p => p.IsTaken == true)
                .ToListAsync();
            
            var tenants = await _context.Users
                .Where(u => u.UserType == "Tenant")
                .ToListAsync();

            // You might want to pass both to view using a ViewModel
            // ViewBag.LeasedProperties = leasedProperties;
            
            return View(tenants);
        }
    }
}