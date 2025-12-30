    using Microsoft.EntityFrameworkCore;
    using property_lease_saas.Data;
    using property_lease_saas.Models.Entities;
    namespace property_lease_saas.Models.Repositories;
    public class MaintenanceApplicationRepository : IMaintenanceApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MaintenanceApplication application)
        {
            _context.MaintenanceApplications.Add(application);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MaintenanceApplication application)
        {
            _context.MaintenanceApplications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task<MaintenanceApplication?> GetByIdAsync(Guid id)
        {
            return await _context.MaintenanceApplications
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<MaintenanceApplication>> ForRequestAsync(Guid requestId)
        {
            return await _context.MaintenanceApplications
                .Where(a => a.MaintenanceRequestId == requestId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceApplication>> ForMechanicAsync(string mechanicId)
        {
            return await _context.MaintenanceApplications
                .Where(a => a.MechanicId == mechanicId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }
    }
