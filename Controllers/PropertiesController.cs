using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Infrastructure.Extensions;
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

        public async Task<IActionResult> My()
        {
            if (!User.IsLandlord()) return Forbid();
            return View(await _repo.GetByLandlordAsync(User.UserId()));
        }

        public IActionResult Create()
        {
            if (!User.IsLandlord()) return Forbid();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Property property,
            List<IFormFile> images,
            List<IFormFile> documents)
        {
            if (!User.IsLandlord()) return Forbid();

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
    public async Task<IActionResult> Delete(Guid id)
    {
        var property = await _repo.GetByIdAsync(id);

        if (property == null)
            return NotFound();

        return View(property); // Pass the property to the view for confirmation
    }

    // POST: Properties/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
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
