using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ProMeet.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
