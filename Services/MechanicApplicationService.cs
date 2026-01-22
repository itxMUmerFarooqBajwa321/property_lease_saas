using property_lease_saas.Data;
using property_lease_saas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Services;

public interface IMechanicApplicationService
{
    Task<Dictionary<string, object>> GetMechanicApplicationsDataAsync(string mechanicId);
}

public class MechanicApplicationService : IMechanicApplicationService
{
    private readonly ApplicationDbContext _context;

    public MechanicApplicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, object>> GetMechanicApplicationsDataAsync(string mechanicId)
    {
        var data = new Dictionary<string, object>();

        // Get all applications by this mechanic
        var applications = await _context.MaintenanceApplications
            .Include(a => a.MaintenanceRequest)
            .Where(a => a.MechanicId == mechanicId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        data["AllApplications"] = applications;
        data["TotalApplications"] = applications.Count;
        data["AcceptedApplications"] = applications.Count(a => a.IsAccepted);
        data["PendingApplications"] = applications.Count(a => !a.IsAccepted && 
            a.MaintenanceRequest.AssignedMechanicId == null);
        data["RejectedApplications"] = applications.Count(a => !a.IsAccepted && 
            a.MaintenanceRequest.AssignedMechanicId != null && 
            a.MaintenanceRequest.AssignedMechanicId != mechanicId);

        // Get assigned/accepted jobs
        var assignedJobs = await _context.MaintenanceRequests
            .Include(mr => mr.Applications)
            .Where(mr => mr.AssignedMechanicId == mechanicId)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync();

        data["AssignedJobs"] = assignedJobs;
        data["TotalAssignedJobs"] = assignedJobs.Count;
        data["InProgressJobs"] = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.InProgress);
        data["CompletedJobs"] = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Verified);
        data["AwaitingStart"] = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Assigned);

        // Get available jobs (published, not assigned)
        var availableJobs = await _context.MaintenanceRequests
            .Where(mr => mr.Status == MaintenanceRequestStatus.Published && 
                        mr.AssignedMechanicId == null)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync();

        // Filter out jobs already applied to
        var appliedJobIds = applications.Select(a => a.MaintenanceRequestId).ToHashSet();
        var unappliedJobs = availableJobs.Where(j => !appliedJobIds.Contains(j.Id)).ToList();

        data["AvailableJobs"] = unappliedJobs;
        data["TotalAvailableJobs"] = unappliedJobs.Count;

        // Calculate earnings
        var acceptedApplications = applications.Where(a => a.IsAccepted).ToList();
        data["TotalEarnings"] = acceptedApplications.Sum(a => a.ProposedBill);
        data["PendingEarnings"] = assignedJobs
            .Where(j => j.Status == MaintenanceRequestStatus.Completed || 
                       j.Status == MaintenanceRequestStatus.InProgress)
            .Join(applications, j => j.Id, a => a.MaintenanceRequestId, (j, a) => a.ProposedBill)
            .Sum();

        // Statistics for charts
        data["ApplicationStats"] = new List<object>
        {
            new { Label = "Accepted", Value = applications.Count(a => a.IsAccepted), Color = "#10b981" },
            new { Label = "Pending", Value = applications.Count(a => !a.IsAccepted && a.MaintenanceRequest.AssignedMechanicId == null), Color = "#f59e0b" },
            new { Label = "Rejected", Value = applications.Count(a => !a.IsAccepted && a.MaintenanceRequest.AssignedMechanicId != null && a.MaintenanceRequest.AssignedMechanicId != mechanicId), Color = "#ef4444" }
        };

        data["JobStats"] = new List<object>
        {
            new { Label = "In Progress", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.InProgress), Color = "#3b82f6" },
            new { Label = "Completed", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Verified), Color = "#10b981" },
            new { Label = "Awaiting Start", Value = assignedJobs.Count(j => j.Status == MaintenanceRequestStatus.Assigned), Color = "#f59e0b" }
        };

        return data;
    }
}