using property_lease_saas.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Authorization;

namespace property_lease_saas.Controllers
{
    [Authorize(Policy = "LandlordOnly")]
    public class TenantsController:Controller
    {
        private readonly ApplicationDbContext _context;

        public IActionResult Index()
        {
            var leasedProperties= _context.Properties
                .Where(p=> p.IsTaken==true)
                .ToList();
            
            var tenants= _context.Users.Where(u => u.UserType== "Tenant").ToList();

            //var tenants = leasedProperties.Where(p=> p==p);
            return View(tenants);   
        }

    }
}