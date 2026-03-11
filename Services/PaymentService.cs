using property_lease_saas.Data;
using property_lease_saas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Services;

public interface IPaymentService
{
    // Rent Payments
    Task<RentPayment> CreateRentPaymentAsync(RentPayment payment);
    Task<RentPayment?> GetRentPaymentByIdAsync(string id);
    Task<List<RentPayment>> GetRentPaymentsByLeaseAsync(string leaseId);
    Task<List<RentPayment>> GetRentPaymentsByTenantAsync(string tenantId);
    Task<List<RentPayment>> GetRentPaymentsByLandlordAsync(string landlordId);
    Task<bool> UpdateRentPaymentStatusAsync(string paymentId, PaymentStatus status, string transactionId = "");
    Task<decimal> GetTotalRentPaidByTenantAsync(string tenantId);
    Task<decimal> GetTotalRentReceivedByLandlordAsync(string landlordId);
    Task<List<RentPayment>> GetPendingRentPaymentsAsync(string userId, string userType);
    Task<List<RentPayment>> GetOverdueRentPaymentsAsync(string landlordId);

    // Maintenance Payments
    Task<MaintenancePayment> CreateMaintenancePaymentAsync(MaintenancePayment payment);
    Task<MaintenancePayment?> GetMaintenancePaymentByIdAsync(string id);
    Task<List<MaintenancePayment>> GetMaintenancePaymentsByMechanicAsync(string mechanicId);
    Task<List<MaintenancePayment>> GetMaintenancePaymentsByLandlordAsync(string landlordId);
    Task<bool> UpdateMaintenancePaymentStatusAsync(string paymentId, PaymentStatus status, string transactionId = "");
    Task<decimal> GetTotalMaintenanceEarningsByMechanicAsync(string mechanicId);
    Task<decimal> GetTotalMaintenancePaidByLandlordAsync(string landlordId);
    Task<List<MaintenancePayment>> GetPendingMaintenancePaymentsAsync(string userId, string userType);

    // Analytics
    Task<Dictionary<string, object>> GetPaymentStatsAsync(string userId, string userType);
}

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Rent Payments

    public async Task<RentPayment> CreateRentPaymentAsync(RentPayment payment)
    {
        // Check if payment is late
        if (payment.PaymentDate > payment.DueDate)
        {
            payment.IsLate = true;
            // Calculate late fee (e.g., 5% of rent amount)
            payment.LateFee = payment.Amount * 0.05m;
        }

        _context.RentPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<RentPayment?> GetRentPaymentByIdAsync(string id)
    {
        return await _context.RentPayments
            .Include(rp => rp.Lease)
            .ThenInclude(l => l.Property)
            .FirstOrDefaultAsync(rp => rp.Id == id);
    }

    public async Task<List<RentPayment>> GetRentPaymentsByLeaseAsync(string leaseId)
    {
        return await _context.RentPayments
            .Where(rp => rp.LeaseId.ToString() == leaseId)
            .OrderByDescending(rp => rp.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<RentPayment>> GetRentPaymentsByTenantAsync(string tenantId)
    {
        return await _context.RentPayments
            .Include(rp => rp.Lease)
            .ThenInclude(l => l.Property)
            .Where(rp => rp.TenantId == tenantId)
            .OrderByDescending(rp => rp.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<RentPayment>> GetRentPaymentsByLandlordAsync(string landlordId)
    {
        return await _context.RentPayments
            .Include(rp => rp.Lease)
            .ThenInclude(l => l.Property)
            .Where(rp => rp.LandlordId == landlordId)
            .OrderByDescending(rp => rp.PaymentDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateRentPaymentStatusAsync(string paymentId, PaymentStatus status, string transactionId = "")
    {
        var payment = await _context.RentPayments.FindAsync(paymentId);
        if (payment == null) return false;

        payment.Status = status;
        if (!string.IsNullOrEmpty(transactionId))
        {
            payment.TransactionId = transactionId;
        }

        if (status == PaymentStatus.Completed)
        {
            payment.PaymentDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalRentPaidByTenantAsync(string tenantId)
    {
        // FIXED: Fetch to client-side first, then sum
        var payments = await _context.RentPayments
            .Where(rp => rp.TenantId == tenantId && rp.Status == PaymentStatus.Completed)
            .Select(rp => rp.Amount)
            .ToListAsync();
        
        return payments.Sum();
    }

    public async Task<decimal> GetTotalRentReceivedByLandlordAsync(string landlordId)
    {
        // FIXED: Fetch to client-side first, then sum
        var payments = await _context.RentPayments
            .Where(rp => rp.LandlordId == landlordId && rp.Status == PaymentStatus.Completed)
            .Select(rp => rp.Amount)
            .ToListAsync();
        
        return payments.Sum();
    }

    public async Task<List<RentPayment>> GetPendingRentPaymentsAsync(string userId, string userType)
    {
        var query = _context.RentPayments
            .Include(rp => rp.Lease)
            .ThenInclude(l => l.Property)
            .Where(rp => rp.Status == PaymentStatus.Pending);

        if (userType == "Tenant")
        {
            query = query.Where(rp => rp.TenantId == userId);
        }
        else if (userType == "Landlord")
        {
            query = query.Where(rp => rp.LandlordId == userId);
        }

        return await query.OrderByDescending(rp => rp.DueDate).ToListAsync();
    }

    public async Task<List<RentPayment>> GetOverdueRentPaymentsAsync(string landlordId)
    {
        return await _context.RentPayments
            .Include(rp => rp.Lease)
            .ThenInclude(l => l.Property)
            .Where(rp => rp.LandlordId == landlordId && 
                        rp.Status == PaymentStatus.Pending && 
                        rp.DueDate < DateTime.Now)
            .OrderByDescending(rp => rp.DueDate)
            .ToListAsync();
    }

    #endregion

    #region Maintenance Payments

    public async Task<MaintenancePayment> CreateMaintenancePaymentAsync(MaintenancePayment payment)
    {   
        _context.MaintenancePayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<MaintenancePayment?> GetMaintenancePaymentByIdAsync(string id)
    {
        return await _context.MaintenancePayments
            .Include(mp => mp.MaintenanceRequest)
            .Include(mp => mp.MaintenanceApplication)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<List<MaintenancePayment>> GetMaintenancePaymentsByMechanicAsync(string mechanicId)
    {
        return await _context.MaintenancePayments
            .Include(mp => mp.MaintenanceRequest)
            .Include(mp => mp.MaintenanceApplication)
            .Where(mp => mp.MechanicId == mechanicId)
            .OrderByDescending(mp => mp.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<MaintenancePayment>> GetMaintenancePaymentsByLandlordAsync(string landlordId)
    {
        return await _context.MaintenancePayments
            .Include(mp => mp.MaintenanceRequest)
            .Include(mp => mp.MaintenanceApplication)
            .Where(mp => mp.LandlordId == landlordId)
            .OrderByDescending(mp => mp.PaymentDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateMaintenancePaymentStatusAsync(string paymentId, PaymentStatus status, string transactionId = "")
    {
        var payment = await _context.MaintenancePayments.FindAsync(paymentId);
        if (payment == null) return false;

        payment.Status = status;
        if (!string.IsNullOrEmpty(transactionId))
        {
            payment.TransactionId = transactionId;
        }

        if (status == PaymentStatus.Completed)
        {
            payment.PaymentDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalMaintenanceEarningsByMechanicAsync(string mechanicId)
    {
        // FIXED: Fetch to client-side first, then sum
        var payments = await _context.MaintenancePayments
            .Where(mp => mp.MechanicId == mechanicId && mp.Status == PaymentStatus.Completed)
            .Select(mp => mp.Amount)
            .ToListAsync();
        
        return payments.Sum();
    }

    public async Task<decimal> GetTotalMaintenancePaidByLandlordAsync(string landlordId)
    {
        // FIXED: Fetch to client-side first, then sum
        var payments = await _context.MaintenancePayments
            .Where(mp => mp.LandlordId == landlordId && mp.Status == PaymentStatus.Completed)
            .Select(mp => mp.Amount)
            .ToListAsync();
        
        return payments.Sum();
    }

    public async Task<List<MaintenancePayment>> GetPendingMaintenancePaymentsAsync(string userId, string userType)
    {
        var query = _context.MaintenancePayments
            .Include(mp => mp.MaintenanceRequest)
            .Include(mp => mp.MaintenanceApplication)
            .Where(mp => mp.Status == PaymentStatus.Pending);

        if (userType == "Mechanic")
        {
            query = query.Where(mp => mp.MechanicId == userId);
        }
        else if (userType == "Landlord")
        {
            query = query.Where(mp => mp.LandlordId == userId);
        }

        return await query.OrderByDescending(mp => mp.CreatedAt).ToListAsync();
    }

    #endregion

    #region Analytics

    public async Task<Dictionary<string, object>> GetPaymentStatsAsync(string userId, string userType)
    {
        var stats = new Dictionary<string, object>();

        if (userType == "Tenant")
        {
            var totalPaid = await GetTotalRentPaidByTenantAsync(userId);
            var pendingPayments = await GetPendingRentPaymentsAsync(userId, userType);
            var allPayments = await GetRentPaymentsByTenantAsync(userId);

            stats["TotalPaid"] = totalPaid;
            stats["PendingPayments"] = pendingPayments.Count;
            stats["PendingAmount"] = pendingPayments.Sum(p => p.Amount);
            stats["TotalPayments"] = allPayments.Count;
            stats["CompletedPayments"] = allPayments.Count(p => p.Status == PaymentStatus.Completed);
            stats["LatePayments"] = allPayments.Count(p => p.IsLate);
            stats["TotalLateFees"] = allPayments.Sum(p => p.LateFee);
        }
        else if (userType == "Landlord")
        {
            var totalReceived = await GetTotalRentReceivedByLandlordAsync(userId);
            var totalPaidToMechanics = await GetTotalMaintenancePaidByLandlordAsync(userId);
            var pendingRentPayments = await GetPendingRentPaymentsAsync(userId, userType);
            var pendingMaintenancePayments = await GetPendingMaintenancePaymentsAsync(userId, userType);
            var overduePayments = await GetOverdueRentPaymentsAsync(userId);

            stats["TotalReceived"] = totalReceived;
            stats["TotalPaidToMechanics"] = totalPaidToMechanics;
            stats["NetIncome"] = totalReceived - totalPaidToMechanics;
            stats["PendingRentPayments"] = pendingRentPayments.Count;
            stats["PendingRentAmount"] = pendingRentPayments.Sum(p => p.Amount);
            stats["PendingMaintenancePayments"] = pendingMaintenancePayments.Count;
            stats["PendingMaintenanceAmount"] = pendingMaintenancePayments.Sum(p => p.Amount);
            stats["OverduePayments"] = overduePayments.Count;
            stats["OverdueAmount"] = overduePayments.Sum(p => p.Amount);
        }
        else if (userType == "Mechanic")
        {
            var totalEarnings = await GetTotalMaintenanceEarningsByMechanicAsync(userId);
            var pendingPayments = await GetPendingMaintenancePaymentsAsync(userId, userType);
            var allPayments = await GetMaintenancePaymentsByMechanicAsync(userId);

            stats["TotalEarnings"] = totalEarnings;
            stats["PendingPayments"] = pendingPayments.Count;
            stats["PendingAmount"] = pendingPayments.Sum(p => p.Amount);
            stats["TotalPayments"] = allPayments.Count;
            stats["CompletedPayments"] = allPayments.Count(p => p.Status == PaymentStatus.Completed);
        }

        return stats;
    }

    #endregion
}