namespace property_lease_saas.Models.Entities;
public enum MaintenanceRequestStatus
{
    Requested,
    Pending,          // Tenant → Landlord
    Published,          // Landlord → Mechanics
    Assigned,           // Landlord accepted mechanic
    InProgress,         // Mechanic started work
    Completed,          // Mechanic finished & uploaded proof
    Verified,           // Landlord verified
    Rejected            // Landlord rejected mechanic application
}