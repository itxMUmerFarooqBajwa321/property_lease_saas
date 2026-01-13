using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace property_lease_saas.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Group users by their UserId for targeted notifications
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        // Landlords join their property groups
        public async Task JoinPropertyGroup(string propertyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"property-{propertyId}");
        }

        // Optional: Remove from groups on disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // You can add cleanup logic here if needed
            await base.OnDisconnectedAsync(exception);
        }
    }
}