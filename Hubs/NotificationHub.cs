using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using property_lease_saas.Models;

namespace property_lease_saas.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Group users by their UserId for targeted notifications
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            Console.WriteLine($"DEBUG: User {userId} joined their group");
        }

        // Landlords join their property groups
        public async Task JoinPropertyGroup(string propertyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"property-{propertyId}");
            Console.WriteLine($"DEBUG: User joined property group {propertyId}");
        }

        // Auto-join mechanics to global mechanic group when they connect
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            
            if (user != null && user.UserType == "Mechanic")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "all-mechanics");
                Console.WriteLine($"DEBUG: Mechanic {user.FullName} ({user.Id}) joined all-mechanics group");
            }
            
            await base.OnConnectedAsync();
        }

        // Optional: Remove from groups on disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}