using property_lease_saas.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Services;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Controllers
{
    [Authorize]
    public class PropertiesController:Controller
    {


        private readonly PropertyService _service;
        private readonly IPropertyRepository _repo;

        public PropertiesController(PropertyService service, IPropertyRepository repo)
        {
            _service = service;
            _repo = repo;
        }

        [Authorize(Policy = "LandlordOnly")]
        public async Task<IActionResult> My()
        {
            return View(await _repo.GetByLandlordAsync(User.UserId()));
        }

        [Authorize(Policy = "LandlordOnly")]
        public IActionResult Create()
        {
            return View();
        }


        [Authorize(Policy = "LandlordOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(
            Property property,
            List<IFormFile> images,
            List<IFormFile> documents)
        {
            property.LandlordId = User.UserId();
            await _service.CreateAsync(property, images, documents);

            return RedirectToAction("My");
        }

        [Authorize (Policy ="TenantOnly")]
        public async Task<IActionResult> Available()
        {
            if (!User.IsTenant()) return Forbid();
            return View(await _repo.GetAvailableAsync());
        }

        // GET: Properties/Delete/5
    
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var property = await _repo.GetByIdAsync(id);

        if (property == null)
            return NotFound();

        return View(property); // Pass the property to the view for confirmation
    }

    // POST: Properties/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var p1= await _repo.GetByIdAsync(id);

        // if (p1==null) ViewBag.message="Not a property!"; // not an existing property 
        // if(p1.IsTaken) ViewBag.message="Property is at lease!" ; // property is at lease

        // if(ViewBag.message != "Not a property!" && ViewBag.message !="Property is at lease!") 
            await _repo.DeleteAsync(id);

        return RedirectToAction(nameof(My));
    }






        public IActionResult Index()
        {
            return View();   
        }

        public async Task<IActionResult> Details(Guid id)
        {
            Property p= await  _repo.GetByIdAsync(id);
            if (p == null)
                return NotFound();
            return View(model:p);
        }
    }
}
