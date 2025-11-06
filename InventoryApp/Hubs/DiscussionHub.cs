using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace InventoryApp.Hubs;

[Authorize] 
public class DiscussionHub : Hub
{
    public async Task JoinInventoryGroup(int inventoryId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"inv-{inventoryId}");

    public async Task LeaveInventoryGroup(int inventoryId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"inv-{inventoryId}");
}
