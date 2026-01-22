using property_lease_saas.Data;
using property_lease_saas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Services;

public interface IDashboardService
{
    Task<Dictionary<string, object>> GetLandlordStatsAsync(string landlordId);
    Task<Dictionary<string, object>> GetTenantStatsAsync(string tenantId);
    Task<Dictionary<string, object>> GetMechanicStatsAsync(string mechanicId);
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public DashboardService(ApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<Dictionary<string, object>> GetLandlordStatsAsync(string landlordId)
    {
        var stats = new Dictionary<string, object>();

        // Properties
        var properties = await _context.Properties
            .Where(p => p.LandlordId == landlordId)
            .ToListAsync();

        stats["TotalProperties"] = properties.Count;
        stats["OccupiedProperties"] = properties.Count(p => p.IsTaken);
        stats["VacantProperties"] = properties.Count(p => !p.IsTaken && p.IsPublished);
        stats["UnpublishedProperties"] = properties.Count(p => !p.IsPublished);

        // Leases
        var leases = await _context.Leases
            .Include(l => l.Property)
            .Where(l => l.LandlordId == landlordId)
            .ToListAsync();

        var activeLeases = leases.Where(l => l.Status == LeaseStatus.Active).ToList();
        stats["ActiveLeases"] = activeLeases.Count;
        stats["TotalRevenue"] = activeLeases.Sum(l => l.RentAmount);
        
        // Expiring leases (next 60 days)
        var expiringLeases = activeLeases
            .Where(l => l.EndDate <= DateTime.Now.AddDays(60))
            .OrderBy(l => l.EndDate)
            .Take(5)
            .ToList();
        stats["ExpiringLeases"] = expiringLeases;
        stats["ExpiringLeasesCount"] = expiringLeases.Count;

        // Lease Requests
        var leaseRequests = await _context.LeaseRequests
            .Include(lr => lr.Property)
            .Where(lr => lr.LandlordId == landlordId)
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();

        stats["PendingLeaseRequests"] = leaseRequests.Count(lr => lr.Status == LeaseRequestStatus.Pending);
        stats["RecentLeaseRequests"] = leaseRequests.Take(5).ToList();

        // Maintenance Requests
        var maintenanceRequests = await _context.MaintenanceRequests
            .Include(mr => mr.Applications)
            .Where(mr => mr.LandlordId == landlordId)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync();

        stats["ActiveMaintenance"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.InProgress || 
            mr.Status == MaintenanceRequestStatus.Published ||
            mr.Status == MaintenanceRequestStatus.Assigned);
        
        stats["CompletedMaintenanceThisMonth"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.Verified && 
            mr.CompletedAt.HasValue && 
            mr.CompletedAt.Value.Month == DateTime.Now.Month);

        stats["PendingVerification"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.Completed);

        stats["RecentMaintenance"] = maintenanceRequests.Take(5).ToList();

        // Monthly revenue chart data (last 6 months)
        var monthlyRevenue = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var month = DateTime.Now.AddMonths(-i);
            monthlyRevenue.Add(new
            {
                Month = month.ToString("MMM"),
                Revenue = activeLeases.Sum(l => l.RentAmount) // Simplified - in real app, calculate actual monthly revenue
            });
        }
        stats["MonthlyRevenueChart"] = monthlyRevenue;

        // Property distribution
        stats["PropertyDistribution"] = new List<object>
        {
            new { Label = "Occupied", Value = properties.Count(p => p.IsTaken), Color = "#10b981" },
            new { Label = "Vacant", Value = properties.Count(p => !p.IsTaken && p.IsPublished), Color = "#f59e0b" },
            new { Label = "Unpublished", Value = properties.Count(p => !p.IsPublished), Color = "#6b7280" }
        };

        stats["Properties"] = properties;
        var paymentStats = await _paymentService.GetPaymentStatsAsync(landlordId, "Landlord");
        stats["TotalRevenue"] = paymentStats["TotalReceived"];
        stats["NetIncome"] = paymentStats["NetIncome"];

        return stats;
    }

    public async Task<Dictionary<string, object>> GetTenantStatsAsync(string tenantId)
    {
        var stats = new Dictionary<string, object>();

        // Active Lease
        var activeLease = await _context.Leases
            .Include(l => l.Property)
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Status == LeaseStatus.Active);

        stats["ActiveLease"] = activeLease;
        stats["HasActiveLease"] = activeLease != null;
        stats["CurrentRent"] = activeLease?.RentAmount ?? 0;
        
        if (activeLease != null)
        {
            var daysUntilExpiry = (activeLease.EndDate - DateTime.Now).Days;
            stats["DaysUntilExpiry"] = daysUntilExpiry;
            stats["LeaseExpiringSoon"] = daysUntilExpiry <= 30;
        }

        // Lease Requests
        var leaseRequests = await _context.LeaseRequests
            .Include(lr => lr.Property)
            .Where(lr => lr.TenantId == tenantId)
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();

        stats["TotalLeaseRequests"] = leaseRequests.Count;
        stats["PendingLeaseRequests"] = leaseRequests.Count(lr => lr.Status == LeaseRequestStatus.Pending);
        stats["ApprovedRequests"] = leaseRequests.Count(lr => lr.Status == LeaseRequestStatus.Approved);
        stats["RejectedRequests"] = leaseRequests.Count(lr => lr.Status == LeaseRequestStatus.Rejected);
        stats["RecentLeaseRequests"] = leaseRequests.Take(5).ToList();

        // Maintenance Requests
        var maintenanceRequests = await _context.MaintenanceRequests
            .Where(mr => mr.TenantId == tenantId)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync();

        stats["TotalMaintenance"] = maintenanceRequests.Count;
        stats["PendingMaintenance"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.Requested || 
            mr.Status == MaintenanceRequestStatus.Pending);
        stats["InProgressMaintenance"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.InProgress);
        stats["CompletedMaintenance"] = maintenanceRequests.Count(mr => 
            mr.Status == MaintenanceRequestStatus.Verified);
        stats["RecentMaintenance"] = maintenanceRequests.Take(5).ToList();

        var paymentStats = await _paymentService.GetPaymentStatsAsync(tenantId, "Tenant");
        stats["PendingRentPayments"] = paymentStats["PendingPayments"];
        stats["PendingRentAmount"] = paymentStats["PendingAmount"];
        // Maintenance status chart
        stats["MaintenanceChart"] = new List<object>
        {
            new { Label = "Pending", Value = maintenanceRequests.Count(mr => mr.Status == MaintenanceRequestStatus.Requested || mr.Status == MaintenanceRequestStatus.Pending), Color = "#f59e0b" },
            new { Label = "In Progress", Value = maintenanceRequests.Count(mr => mr.Status == MaintenanceRequestStatus.InProgress), Color = "#3b82f6" },
            new { Label = "Completed", Value = maintenanceRequests.Count(mr => mr.Status == MaintenanceRequestStatus.Verified), Color = "#10b981" }
        };


        return stats;
    }

    public async Task<Dictionary<string, object>> GetMechanicStatsAsync(string mechanicId)
    {
        var stats = new Dictionary<string, object>();
        

        // Applications
        var applications = await _context.MaintenanceApplications
            .Include(a => a.MaintenanceRequest)
            .Where(a => a.MechanicId == mechanicId)
            .ToListAsync();

        stats["TotalApplications"] = applications.Count;
        stats["AcceptedApplications"] = applications.Count(a => a.IsAccepted);
        stats["PendingApplications"] = applications.Count(a => !a.IsAccepted && 
            a.MaintenanceRequest.AssignedMechanicId == null);
        stats["RejectedApplications"] = applications.Count(a => !a.IsAccepted && 
            a.MaintenanceRequest.AssignedMechanicId != null && 
            a.MaintenanceRequest.AssignedMechanicId != mechanicId);
        stats["RecentApplications"] = applications.OrderByDescending(a => a.AppliedAt).Take(5).ToList();

        // Assigned Jobs
        var assignedJobs = await _context.MaintenanceRequests
            .Where(mr => mr.AssignedMechanicId == mechanicId)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync();

        stats["TotalAssignedJobs"] = assignedJobs.Count;
        stats["InProgressJobs"] = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.InProgress);
        stats["CompletedJobs"] = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Verified);
        stats["CompletedThisMonth"] = assignedJobs.Count(j => 
            j.Status == MaintenanceRequestStatus.Verified && 
            j.CompletedAt.HasValue && 
            j.CompletedAt.Value.Month == DateTime.Now.Month);
        stats["AssignedJobs"] = assignedJobs.Take(5).ToList();

        // Available Jobs
        var availableJobs = await _context.MaintenanceRequests
            .Where(mr => mr.Status == MaintenanceRequestStatus.Published && 
                        mr.AssignedMechanicId == null)
            .OrderByDescending(mr => mr.CreatedAt)
            .Take(10)
            .ToListAsync();

        stats["AvailableJobs"] = availableJobs.Count;
        stats["AvailableJobsList"] = availableJobs;

        // Earnings
        var totalEarnings = applications.Where(a => a.IsAccepted).Sum(a => a.ProposedBill);
        stats["TotalEarnings"] = totalEarnings;
        stats["PendingPayment"] = assignedJobs
            .Where(j => j.Status == MaintenanceRequestStatus.Completed || j.Status == MaintenanceRequestStatus.Verified)
            .Join(applications, 
                  j => j.Id, 
                  a => a.MaintenanceRequestId, 
                  (j, a) => a.ProposedBill)
            .Sum();

        // Job status chart
        stats["JobStatusChart"] = new List<object>
        {
            new { Label = "In Progress", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.InProgress), Color = "#f59e0b" },
            new { Label = "Completed", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Verified), Color = "#10b981" },
            new { Label = "Pending", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Assigned), Color = "#6b7280" }
        };

        var paymentStats = await _paymentService.GetPaymentStatsAsync(mechanicId, "Mechanic");
        stats["TotalEarnings"] = paymentStats["TotalEarnings"];
        stats["PendingPayments"] = paymentStats["PendingPayments"];

        // Monthly earnings (last 6 months)
        var monthlyEarnings = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var month = DateTime.Now.AddMonths(-i);
            var monthEarnings = assignedJobs
                .Where(j => j.CompletedAt.HasValue && 
                           j.CompletedAt.Value.Month == month.Month && 
                           j.CompletedAt.Value.Year == month.Year)
                .Join(applications, j => j.Id, a => a.MaintenanceRequestId, (j, a) => a.ProposedBill)
                .Sum();
            
            monthlyEarnings.Add(new
            {
                Month = month.ToString("MMM"),
                Earnings = monthEarnings
            });
        }
        stats["MonthlyEarningsChart"] = monthlyEarnings;

        return stats;
    }
}