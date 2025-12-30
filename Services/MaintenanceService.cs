using property_lease_saas.Models.Repositories;
using property_lease_saas.Models.Entities;

namespace property_lease_saas.Services;

public class MaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IMaintenanceApplicationRepository _appRepo;
    private readonly IFileStorage _fileStorage;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepo,
        IMaintenanceApplicationRepository appRepo,
        IFileStorage fileStorage)
    {
        _maintenanceRepo = maintenanceRepo;
        _appRepo = appRepo;
        _fileStorage = fileStorage;
    }

    // ================= TENANT =================
    public async Task CreateRequestAsync(
        Guid leaseId,
        Guid propertyId,
        string tenantId,
        string landlordId,
        string title,
        string desc)
    {
        var req = new MaintenanceRequest
        {
            Id = Guid.NewGuid(),
            LeaseId = leaseId,
            PropertyId = propertyId,
            TenantId = tenantId,
            LandlordId = landlordId,
            Title = title,
            Description = desc,
            Status = MaintenanceRequestStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };

        await _maintenanceRepo.AddAsync(req);
    }

    // ================= LANDLORD =================
    public async Task PublishAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req == null) throw new Exception("Request not found");

        req.Status = MaintenanceRequestStatus.Published;
        await _maintenanceRepo.UpdateAsync(req);
    }

    // ================= MECHANIC =================
    public async Task ApplyAsync(
        Guid requestId,
        string mechanicId,
        decimal bill,
        string notes)
    {
        var app = new MaintenanceApplication
        {
            Id = Guid.NewGuid(),
            MaintenanceRequestId = requestId,
            MechanicId = mechanicId,
            ProposedBill = bill,
            Notes = notes,
            IsAccepted = true,
            AppliedAt = DateTime.UtcNow
        };

        await _appRepo.AddAsync(app);
    }

    // ================= LANDLORD =================
    public async Task AcceptMechanicAsync(Guid applicationId)
    {
        var app = await _appRepo.GetByIdAsync(applicationId);
        if (app == null) throw new Exception("Application not found");

        app.IsAccepted = true;

        var req = await _maintenanceRepo.GetByIdAsync(app.MaintenanceRequestId);
        req.AssignedMechanicId = Guid.Parse(app.MechanicId);
        req.Status = MaintenanceRequestStatus.Assigned;

        await _appRepo.UpdateAsync(app);
        await _maintenanceRepo.UpdateAsync(req);
    }

    // ================= MECHANIC =================
    public async Task StartWorkAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req.Status != MaintenanceRequestStatus.Assigned)
            throw new Exception("Work not assigned");

        req.Status = MaintenanceRequestStatus.InProgress;
        await _maintenanceRepo.UpdateAsync(req);
    }

    // ================= MECHANIC (UPLOAD FILES) =================
    public async Task CompleteAsync(
        Guid requestId,
        IFormFile receipt,
        IFormFile completionImage)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req.Status != MaintenanceRequestStatus.InProgress)
            throw new Exception("Work not in progress");

        req.ReceiptPath = await _fileStorage.SaveAsync(
            receipt,
            "uploads/maintenance/receipts");

        req.CompletionImagePath = await _fileStorage.SaveAsync(
            completionImage,
            "uploads/maintenance/completions");

        req.Status = MaintenanceRequestStatus.Completed;
        req.CompletedAt = DateTime.UtcNow;

        await _maintenanceRepo.UpdateAsync(req);
    }

    // ================= LANDLORD =================
    public async Task VerifyAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req.Status != MaintenanceRequestStatus.Completed)
            throw new Exception("Work not completed");

        req.Status = MaintenanceRequestStatus.Verified;
        await _maintenanceRepo.UpdateAsync(req);
    }

    // ================= MECHANIC =================
    public async Task UploadCompletionAsync(Guid requestId, IFormFile receipt, IFormFile completionImage)
    {
        var request = await _maintenanceRepo.GetByIdAsync(requestId);

        if (request == null)
            throw new Exception("Maintenance request not found");

        if (request.Status != MaintenanceRequestStatus.InProgress)
            throw new Exception("Work not in progress");

        var receiptPath = await _fileStorage.SaveAsync(
            receipt,
            "uploads/maintenance/receipts");

        var imagePath = await _fileStorage.SaveAsync(
            completionImage,
            "uploads/maintenance/completions");

        request.ReceiptPath = receiptPath;
        request.CompletionImagePath = imagePath;
        request.Status = MaintenanceRequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;

        await _maintenanceRepo.UpdateAsync(request);
    }

    // ================= QUERIES =================
    
    // TENANT
    public Task<List<MaintenanceRequest>> ForTenantAsync(string tenantId)
    {
        return _maintenanceRepo.GetForTenantAsync(tenantId);
    }

    // LANDLORD
    public Task<List<MaintenanceRequest>> ForLandlordAsync(string landlordId)
    {
        return _maintenanceRepo.GetForLandlordAsync(landlordId);
    }

    // MECHANIC
    public Task<List<MaintenanceRequest>> PublishedAsync()
    {
        return _maintenanceRepo.GetPublishedAsync();
    }

    public Task<List<MaintenanceRequest>> ForMechanicAsync(string mechanicId)
    {
        return _maintenanceRepo.GetForMechanicAsync(mechanicId);
    }

    // ================= LANDLORD-INITIATED REQUEST =================
    // ================= LANDLORD-INITIATED REQUEST =================
public async Task RequestAsync(
    string landlordId,
    string title,
    string description,
    Guid propertyId)
{
    var request = new MaintenanceRequest
    {
        Id = Guid.NewGuid(),
        Title = title,
        Description = description,
        PropertyId = propertyId,
        LandlordId = landlordId,
        TenantId = null, // No tenant for landlord-initiated requests
        LeaseId = null, // No lease for landlord-initiated requests
        Status = MaintenanceRequestStatus.Requested,
        CreatedAt = DateTime.UtcNow
    };

    await _maintenanceRepo.AddAsync(request);
}
}