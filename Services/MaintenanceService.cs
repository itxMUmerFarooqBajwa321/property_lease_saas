using property_lease_saas.Data;
using property_lease_saas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Models.Repositories;

namespace property_lease_saas.Services;

public class MaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IMaintenanceApplicationRepository _appRepo;
    private readonly IFileStorage _fileStorage;
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepo,
        IMaintenanceApplicationRepository appRepo,
        IFileStorage fileStorage,
        INotificationService notificationService,
        ApplicationDbContext context)
    {
        _maintenanceRepo = maintenanceRepo;
        _appRepo = appRepo;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
        _context = context;
    }

    public IMaintenanceRepository GetMaintenanceRepo()
    {
        return _maintenanceRepo;
    }

    // ================= TENANT =================
    public IMaintenanceApplicationRepository GetAppRepo()
    {
        return _appRepo;
    }

    // 1️⃣ TENANT CREATES REQUEST
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
        
        // ADD NOTIFICATION
        var tenant = await _context.Users.FirstOrDefaultAsync(u => u.Id == tenantId);
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        var tenantName = tenant?.FullName ?? "A tenant";
        var propertyTitle = property?.Title ?? "your property";
        
        Console.WriteLine($"DEBUG: Sending maintenance request created notification");
        
        await _notificationService.NotifyMaintenanceRequestCreated(
            req.Id, 
            tenantId, 
            tenantName, 
            propertyTitle, 
            landlordId);
    }

    // ================= LANDLORD =================
    
    // 2️⃣ LANDLORD PUBLISHES
    public async Task PublishAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req == null) throw new Exception("Request not found");

        req.Status = MaintenanceRequestStatus.Published;
        await _maintenanceRepo.UpdateAsync(req);
        
        // ADD NOTIFICATION
        Console.WriteLine($"DEBUG: Publishing maintenance request {requestId}");
        await _notificationService.NotifyMaintenanceRequestPublished(requestId);
    }

    // ================= MECHANIC =================
    
    // 3️⃣ MECHANIC APPLIES
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
            IsAccepted = false,
            AppliedAt = DateTime.UtcNow
        };
        
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        req.Applications.Add(app);
        await _appRepo.AddAsync(app);
        
        // ADD NOTIFICATION
        var mechanic = await _context.Users.FirstOrDefaultAsync(u => u.Id == mechanicId);
        var mechanicName = mechanic?.FullName ?? "A mechanic";
        
        Console.WriteLine($"DEBUG: Mechanic {mechanicName} applied for request {requestId}");
        
        await _notificationService.NotifyMaintenanceApplicationReceived(
            app.Id, 
            requestId, 
            mechanicId, 
            mechanicName, 
            bill);
    }

    // ================= LANDLORD =================
    
    // 4️⃣ LANDLORD ACCEPTS MECHANIC
    public async Task AcceptMechanicAsync(Guid applicationId)
    {
        var app = await _appRepo.GetByIdAsync(applicationId);
        if (app == null) throw new Exception("Application not found");

        app.IsAccepted = true;

        var req = await _maintenanceRepo.GetByIdAsync(app.MaintenanceRequestId);
        
        // CHANGE THIS LINE - remove Guid.Parse()
        req.AssignedMechanicId = app.MechanicId;  // ✅ Changed from Guid.Parse(app.MechanicId)
        
        req.Status = MaintenanceRequestStatus.Assigned;

        await _appRepo.UpdateAsync(app);
        await _maintenanceRepo.UpdateAsync(req);
        
        // Notification code...
        var mechanic = await _context.Users.FirstOrDefaultAsync(u => u.Id == app.MechanicId);
        var mechanicName = mechanic?.FullName ?? "Mechanic";
        
        Console.WriteLine($"DEBUG: Landlord accepted mechanic {mechanicName}");
        
        await _notificationService.NotifyMaintenanceApplicationAccepted(
            req.Id, 
            app.MechanicId, 
            mechanicName);
    }

    // ================= MECHANIC =================
    
    // 5️⃣ MECHANIC STARTS WORK
    public async Task StartWorkAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req.Status != MaintenanceRequestStatus.Assigned)
            throw new Exception("Work not assigned");

        req.Status = MaintenanceRequestStatus.InProgress;
        await _maintenanceRepo.UpdateAsync(req);
        
        // ADD NOTIFICATION
        Console.WriteLine($"DEBUG: Mechanic started work on request {requestId}");
        await _notificationService.NotifyMaintenanceWorkStarted(requestId);
    }

    // 6️⃣ MECHANIC COMPLETES WORK
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
        
        // ADD NOTIFICATION
        Console.WriteLine($"DEBUG: Mechanic completed work on request {requestId}");
        await _notificationService.NotifyMaintenanceWorkCompleted(requestId);
    }

    // ================= LANDLORD =================
    
    // 7️⃣ LANDLORD VERIFIES
    public async Task VerifyAsync(Guid requestId)
    {
        var req = await _maintenanceRepo.GetByIdAsync(requestId);
        if (req.Status != MaintenanceRequestStatus.Completed)
            throw new Exception("Work not completed");

        req.Status = MaintenanceRequestStatus.Verified;
        await _maintenanceRepo.UpdateAsync(req);
        
        // ADD NOTIFICATION
        Console.WriteLine($"DEBUG: Landlord verified request {requestId}");
        await _notificationService.NotifyMaintenanceWorkVerified(requestId);
    }

    // ================= MECHANIC (UPLOAD FILES) =================
    public async Task UploadCompletionAsync(
        Guid requestId,
        IFormFile receipt,
        IFormFile completionImage)
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
        
        // ADD NOTIFICATION
        Console.WriteLine($"DEBUG: Mechanic uploaded completion for request {requestId}");
        await _notificationService.NotifyMaintenanceWorkCompleted(requestId);
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
        
        // No notification needed since landlord created it themselves
        Console.WriteLine($"DEBUG: Landlord created maintenance request {request.Id}");
    }
}

