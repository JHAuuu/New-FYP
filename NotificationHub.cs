using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;

namespace fyp
{
    public class NotificationHub : Hub
    {
        // Concurrent dictionary to store userId to connectionId mapping
        private static readonly ConcurrentDictionary<int, string> UserConnections = new ConcurrentDictionary<int, string>();
        // Define methods that clients can call
        public void SendMessage(string message)
        {
            Clients.All.broadcastMessage(message); // Notify all connected clients
        }

        // This is where you handle the announcement notification
        public void ReceiveAnnouncement(string title, string content)
        {
            Clients.All.receiveAnnouncement(title, content);
        }

        // Get a connection ID for a specific user
        public static string GetConnectionId(int userId)
        {
            if (UserConnections.TryGetValue(userId, out string connectionId))
            {
                return connectionId;
            }
            return null;
        }

        public override Task OnConnected()
        {
            string connectionId = Context.ConnectionId;
            int userId = Convert.ToInt32(Context.QueryString["userId"]);

            // Map the userId to the connectionId
            UserConnections[userId] = connectionId;

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            int userId = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                UserConnections.TryRemove(userId, out _);
            }

            return base.OnDisconnected(stopCalled);
        }

        public void ReceiveInbox(int userId, string title, string content, string sendAt)
        {
            System.Diagnostics.Debug.WriteLine($"ReceiveInbox triggered for UserId {userId}: {title}, {content}, {sendAt}");
            if (UserConnections.TryGetValue(userId, out string connectionId))
            {
                Clients.Client(connectionId).broadcastInbox(title, content, sendAt);
            }
        }
    }
}