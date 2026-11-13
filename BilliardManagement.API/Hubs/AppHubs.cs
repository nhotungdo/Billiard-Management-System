using Microsoft.AspNetCore.SignalR;

namespace BilliardManagement.API.Hubs
{
    public abstract class StoreHubBase : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var group = StoreGroups.FromUser(Context.User);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = StoreGroups.FromUser(Context.User);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await base.OnDisconnectedAsync(exception);
        }
    }

    public class TableHub : StoreHubBase
    {
        public async Task SendTableUpdate(string message)
        {
            await Clients.Group(StoreGroups.FromUser(Context.User)).SendAsync("ReceiveTableUpdate", message);
        }
    }

    public class ProductHub : StoreHubBase
    {
        public async Task SendProductUpdate(string message)
        {
            await Clients.Group(StoreGroups.FromUser(Context.User)).SendAsync("ReceiveProductUpdate", message);
        }
    }

    public class OrderHub : StoreHubBase
    {
        public async Task SendOrderUpdate(string message)
        {
            await Clients.Group(StoreGroups.FromUser(Context.User)).SendAsync("ReceiveOrderUpdate", message);
        }
    }

    public class NotificationHub : StoreHubBase
    {
        public async Task SendNotification(string message)
        {
            await Clients.Group(StoreGroups.FromUser(Context.User)).SendAsync("ReceiveNotification", message);
        }
    }
}
