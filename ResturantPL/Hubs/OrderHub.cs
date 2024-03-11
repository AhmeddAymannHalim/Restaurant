
using Microsoft.AspNetCore.SignalR;
namespace ResturantPL.Hubs
{
    public class OrderHub : Hub
    {
        public async Task RefreshOrders()
        {
            await Clients.All.SendAsync("RefreshOrders");
        }
        
        
        public async Task RefreshOnlineOrders()
        {
            await Clients.All.SendAsync("RefreshOnlineOrders");
        }
    }
}
