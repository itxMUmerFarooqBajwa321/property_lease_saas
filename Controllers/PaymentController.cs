using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using property_lease_saas.Infrastructure.Extensions;
using property_lease_saas.Models.Entities;
using property_lease_saas.Services;
using property_lease_saas.Data;
using Microsoft.EntityFrameworkCore;

namespace property_lease_saas.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;

    public PaymentController(IPaymentService paymentService, ApplicationDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    #region Rent Payments (Tenant -> Landlord)

    // Tenant: View their payments
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> MyRentPayments()
    {
        var userId = User.UserId();
        var payments = await _paymentService.GetRentPaymentsByTenantAsync(userId);
        var stats = await _paymentService.GetPaymentStatsAsync(userId, "Tenant");

        ViewBag.Stats = stats;
        return View(payments);
    }

    [HttpGet]
    // Tenant: Make a payment
    [Authorize(Policy = "TenantOnly")]
    public async Task<IActionResult> MakeRentPayment(string leaseId)
    {
        // Validate input
        if (string.IsNullOrEmpty(leaseId))
        {
            TempData["Error"] = "Lease ID is required";
            return RedirectToAction("MyRentPayments");
        }

        // Try to parse as Guid
        if (!Guid.TryParse(leaseId, out var leaseGuid))
        {
            // If not a Guid, maybe it's stored without hyphens
            if (leaseId.Length == 32) // Guid without hyphens is 32 chars
            {
                var formattedId = $"{leaseId.Substring(0, 8)}-{leaseId.Substring(8, 4)}-{leaseId.Substring(12, 4)}-{leaseId.Substring(16, 4)}-{leaseId.Substring(20)}";
                if (Guid.TryParse(formattedId, out leaseGuid))
                {
                    Console.WriteLine($"DEBUG: Successfully parsed without-hyphens GUID: {leaseId} -> {leaseGuid}");
                }
            }

            if (leaseGuid == Guid.Empty)
            {
                TempData["Error"] = "Invalid Lease ID format";
                return RedirectToAction("MyRentPayments");
            }
        }

        // Now query with Guid
        var lease = await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.LeaseRequest)
            .FirstOrDefaultAsync(l => l.Id == leaseGuid);

        if (lease == null)
        {
            TempData["Error"] = $"Lease not found. Looking for ID: {leaseGuid}";
            return RedirectToAction("MyRentPayments");
        }

        if (lease.TenantId != User.UserId())
        {
            TempData["Error"] = "You are not authorized to make payments for this lease";
            return RedirectToAction("MyRentPayments");
        }

        ViewBag.Lease = lease;

        var payment = new RentPayment
        {
            LeaseId = lease.Id,
            Amount = lease.RentAmount,
            DueDate = DateTime.Now.AddDays(7)
        };

        return View(payment);
    }

    [HttpPost]
    [Authorize(Policy = "TenantOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeRentPayment(RentPayment payment)
    {
        var lease = await _context.Leases
            .Include(l => l.Property)
            .FirstOrDefaultAsync(l => l.Id == payment.LeaseId);

        if (!ModelState.IsValid)
        {
            ViewBag.Lease = lease;
            return View(payment);
        }

        if (lease == null || lease.TenantId != User.UserId())
        {
            TempData["Error"] = "Lease not found or unauthorized";
            return RedirectToAction("MyRentPayments");
        }

        payment.TenantId = User.UserId();
        payment.TenantName = User.Identity?.Name ?? "";
        payment.LandlordId = lease.LandlordId;
        payment.LandlordName = lease.Property?.LandlordId ?? "";
        payment.Amount = lease.RentAmount;
        payment.PaymentDate = DateTime.Now;
        payment.Status = PaymentStatus.Pending; // Or Completed based on your logic


        await _paymentService.CreateRentPaymentAsync(payment);

        TempData["Success"] = "Payment submitted successfully!";
        return RedirectToAction("MyRentPayments");
    }

    // Landlord: View received payments
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> ReceivedRentPayments()
    {
        var userId = User.UserId();
        var payments = await _paymentService.GetRentPaymentsByLandlordAsync(userId);
        var stats = await _paymentService.GetPaymentStatsAsync(userId, "Landlord");

        ViewBag.Stats = stats;
        return View(payments);
    }

    // Landlord: View overdue payments
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> OverduePayments()
    {
        var userId = User.UserId();
        var payments = await _paymentService.GetOverdueRentPaymentsAsync(userId);
        return View(payments);
    }

    // Landlord: Confirm payment received
    [HttpPost]
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> ConfirmRentPayment(string paymentId, string transactionId)
    {
        var payment = await _paymentService.GetRentPaymentByIdAsync(paymentId);

        if (payment == null || payment.LandlordId != User.UserId())
        {
            return Json(new { success = false, message = "Payment not found or unauthorized" });
        }

        var result = await _paymentService.UpdateRentPaymentStatusAsync(
            paymentId,
            PaymentStatus.Completed,
            transactionId
        );

        if (result)
        {
            return Json(new { success = true, message = "Payment confirmed successfully!" });
        }

        return Json(new { success = false, message = "Failed to confirm payment" });
    }

    #endregion

    #region Maintenance Payments (Landlord -> Mechanic)

    // Landlord: Create payment for mechanic
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> CreateMaintenancePayment(string maintenanceRequestId)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .Include(mr => mr.Applications)
            .ThenInclude(a => a.MechanicId)
            .FirstOrDefaultAsync(mr => mr.Id.ToString() == maintenanceRequestId);
        
        //MaintenancePayment payment = new MaintenancePayment() create an object of maintenacepaymnet and pass it in method written in next line to write this payment in DB
        //_paymentService.CreateMaintenancePaymentAsync();  

        if (maintenanceRequest == null || maintenanceRequest.LandlordId != User.UserId())
        {
            TempData["Error"] = "Maintenance request not found or unauthorized";
            Console.WriteLine(TempData["Error"]);
            return RedirectToAction("PaidMaintenancePayments");
        }

        if (maintenanceRequest.Status != MaintenanceRequestStatus.Verified)
        {
            TempData["Error"] = "Maintenance work must be verified before payment";
            Console.WriteLine(TempData["Error"]);
            return RedirectToAction("PaidMaintenancePayments");
        }

        var acceptedApplication = maintenanceRequest.Applications
            .FirstOrDefault(a => a.IsAccepted);

        if (acceptedApplication == null)
        {
            TempData["Error"] = "No accepted application found";
            Console.WriteLine(TempData["Error"]);
            return RedirectToAction("PaidMaintenancePayments");
        }

        ViewBag.MaintenanceRequest = maintenanceRequest;
        ViewBag.Application = acceptedApplication;

        return View();
    }

    [HttpPost]
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> CreateMaintenancePayment(MaintenancePayment payment)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .Include(mr => mr.Applications)
            .FirstOrDefaultAsync(mr => mr.Id == payment.MaintenanceApplicationId);

        if (maintenanceRequest == null || maintenanceRequest.LandlordId != User.UserId())
        {
            TempData["Error"] = "Maintenance request not found or unauthorized";
            return RedirectToAction("PaidMaintenancePayments");
        }

        var application = await _context.MaintenanceApplications
            .Include(a => a.MechanicId)
            .FirstOrDefaultAsync(a => a.Id == payment.MaintenanceApplicationId);

        if (application == null || !application.IsAccepted)
        {
            TempData["Error"] = "Application not found or not accepted";
            return RedirectToAction("PaidMaintenancePayments");
        }

        payment.LandlordId = User.UserId();
        payment.LandlordName = User.Identity?.Name ?? "";
        payment.MechanicId = application.MechanicId;
        payment.MechanicName = application.MechanicId ?? "";
        payment.WorkDescription = maintenanceRequest.Title;

        await _paymentService.CreateMaintenancePaymentAsync(payment);

        TempData["Success"] = "Payment submitted successfully!";
        return RedirectToAction("PaidMaintenancePayments");
    }

    // Landlord: View payments to mechanics
    [Authorize(Policy = "LandlordOnly")]
    public async Task<IActionResult> PaidMaintenancePayments()
    {
        var userId = User.UserId();
        var payments = await _paymentService.GetMaintenancePaymentsByLandlordAsync(userId);
        var stats = await _paymentService.GetPaymentStatsAsync(userId, "Landlord");

        ViewBag.Stats = stats;
        return View(payments);
    }

    // Mechanic: View received payments
    [Authorize(Policy = "MechanicOnly")]
    public async Task<IActionResult> MyEarnings()
    {
        var userId = User.UserId();
        var payments = await _paymentService.GetMaintenancePaymentsByMechanicAsync(userId);
        var stats = await _paymentService.GetPaymentStatsAsync(userId, "Mechanic");

        ViewBag.Stats = stats;
        return View(payments);
    }

    // Mechanic: Confirm payment received
    [HttpPost]
    [Authorize(Policy = "MechanicOnly")]
    public async Task<IActionResult> ConfirmMaintenancePaymentReceived(string paymentId)
    {
        var payment = await _paymentService.GetMaintenancePaymentByIdAsync(paymentId);

        if (payment == null || payment.MechanicId != User.UserId())
        {
            return Json(new { success = false, message = "Payment not found or unauthorized" });
        }

        var result = await _paymentService.UpdateMaintenancePaymentStatusAsync(
            paymentId,
            PaymentStatus.Completed
        );

        if (result)
        {
            return Json(new { success = true, message = "Payment confirmed successfully!" });
        }

        return Json(new { success = false, message = "Failed to confirm payment" });
    }

    #endregion

    #region Common Actions

    // View payment details
    public async Task<IActionResult> PaymentDetails(string id, string type)
    {
        var userType = User.FindFirst("UserType")?.Value;
        var userId = User.UserId();

        if (type == "rent")
        {
            var payment = await _paymentService.GetRentPaymentByIdAsync(id);

            if (payment == null)
            {
                TempData["Error"] = "Payment not found";
                return RedirectToAction("Index", "Dashboard");
            }

            // Check authorization
            if ((userType == "Tenant" && payment.TenantId != userId) ||
                (userType == "Landlord" && payment.LandlordId != userId))
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.PaymentType = "Rent";
            return View("RentPaymentDetails", payment);
        }
        else if (type == "maintenance")
        {
            var payment = await _paymentService.GetMaintenancePaymentByIdAsync(id);

            if (payment == null)
            {
                TempData["Error"] = "Payment not found";
                return RedirectToAction("Index", "Dashboard");
            }

            // Check authorization
            if ((userType == "Mechanic" && payment.MechanicId != userId) ||
                (userType == "Landlord" && payment.LandlordId != userId))
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.PaymentType = "Maintenance";
            return View("MaintenancePaymentDetails", payment);
        }

        TempData["Error"] = "Invalid payment type";
        return RedirectToAction("Index", "Dashboard");
    }

    // Cancel payment (only pending payments)
    [HttpPost]
    public async Task<IActionResult> CancelPayment(string id, string type)
    {
        var userType = User.FindFirst("UserType")?.Value;
        var userId = User.UserId();

        if (type == "rent")
        {
            var payment = await _paymentService.GetRentPaymentByIdAsync(id);

            if (payment == null || payment.TenantId != userId)
            {
                return Json(new { success = false, message = "Payment not found or unauthorized" });
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Json(new { success = false, message = "Only pending payments can be cancelled" });
            }

            var result = await _paymentService.UpdateRentPaymentStatusAsync(id, PaymentStatus.Cancelled);

            if (result)
            {
                return Json(new { success = true, message = "Payment cancelled successfully" });
            }
        }
        else if (type == "maintenance")
        {
            var payment = await _paymentService.GetMaintenancePaymentByIdAsync(id);

            if (payment == null || payment.LandlordId != userId)
            {
                return Json(new { success = false, message = "Payment not found or unauthorized" });
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Json(new { success = false, message = "Only pending payments can be cancelled" });
            }

            var result = await _paymentService.UpdateMaintenancePaymentStatusAsync(id, PaymentStatus.Cancelled);

            if (result)
            {
                return Json(new { success = true, message = "Payment cancelled successfully" });
            }
        }

        return Json(new { success = false, message = "Failed to cancel payment" });
    }

    #endregion
}