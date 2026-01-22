using property_lease_saas.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[Authorize]
public class ChatHub:Hub
{
    // [HttpPost]
    public async Task SendMessage(string SenderId, string ReceiverId, string Message, DateTime d)
    {
        await Clients.User(ReceiverId).SendAsync("ReceiveMessage");
        Console.WriteLine(Message);
    }

}