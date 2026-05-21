using Microsoft.AspNetCore.SignalR;

namespace BilliardManagement.API.Hubs
{
    public class TableHub : Hub
    {
        public async Task SendTableUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveTableUpdate", message);
        }
    }

    public class ProductHub : Hub
    {
        public async Task SendProductUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveProductUpdate", message);
        }
    }

    public class OrderHub : Hub
    {
        public async Task SendOrderUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", message);
        }
    }

    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
