using Flame_Social_Network_Web_App.Data;
using Flame_Social_Network_Web_App.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public static HashSet<ConnectionData> CurrentConnections = new HashSet<ConnectionData>(); // here we are storing all of the connected clients' connections id
        public static HashSet<ConnectionData> LostConnections = new HashSet<ConnectionData>(); // there goes all disconnected connections and will be removed after 2 seconds of waiting for reconnection
        
        static HashSet<ChatRoom> ChatRooms = new HashSet<ChatRoom>(); // Every active chatroom goes here

        public override async Task OnConnectedAsync()
        {
            var connection = new ConnectionData { ConnectionId = Context.ConnectionId, Tag = Context.User.Identity.Name }; //Context.User.Identity.Name };

            CurrentConnections.Add(connection);
            if(LostConnections.Contains(connection))
            {
                // then it's a lost connection
                LostConnections.Remove(connection);
            }
            await Clients.All.SendAsync("UserConnected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connection = GetConnectionDataByTag(Context.User.Identity.Name);
            if(connection != null)
            {
                Task task = ToBeRemoved(connection);
                task.Start();
            } // else the connection wasn't stored in the current connections
            await base.OnDisconnectedAsync(exception);
        }

        public Task ListContacts()
        {
            return Clients.Caller.SendAsync("ListContacts", GetContactsStringListFor(Context.User.Identity.Name));
        }

        Task ToBeRemoved(ConnectionData connectionData)
        {
            if(CurrentConnections.Contains(connectionData))
            {
                CurrentConnections.Remove(connectionData);
                LostConnections.Add(connectionData);
            }
            // We wait some time for the user to reconnect
            Thread.Sleep(Constants.MaxReconnectWaitTime);

            // If the user still hasn't reconnected
            if(LostConnections.Contains(connectionData))
            {
                // Then we have to stop waiting for reconnection
                LostConnections.Remove(connectionData);
                Clients.All.SendAsync("UserDisconnected");
            }
            return Task.CompletedTask;
        }

        public Task ChatWith(string tag)
        {
            // tag validation
            if(Context.User.Identity.Name.Equals(tag))
            {
                // we can't chat with ourselves
            }
            if(!IsConnected(tag))
            {
                // then we can't talk
            }
            return Task.CompletedTask;
        }

        ///////////// Tools /////////////
        ConnectionData GetConnectionDataByTag(string tag)
        {
            return CurrentConnections.FirstOrDefault(c => c.Tag.Equals(tag));
        }

        public static List<string> GetContactsStringListFor(string tag)
        {
            // we are reading from this list for now
            List<string> list = new List<string>();
            foreach(ConnectionData connection in CurrentConnections)
            {
                if(!connection.Tag.Equals(tag))
                {
                    list.Add(connection.Tag);
                }
            }
            foreach (ConnectionData connection in LostConnections)
            {
                if (!connection.Tag.Equals(tag))
                {
                    list.Add(connection.Tag);
                }
            }
            list.Sort();
            return list;
        }

        public static ContactsList GetContactsListFor(string tag)
        {
            // we are reading from this list for now
            ContactsList list = new ContactsList();
            foreach (ConnectionData connection in CurrentConnections)
            {
                if (!connection.Tag.Equals(tag))
                {
                    list.List.Add(new Contact(connection));
                }
            }
            foreach (ConnectionData connection in LostConnections)
            {
                if (!connection.Tag.Equals(tag))
                {
                    list.List.Add(new Contact(connection));
                }
            }
            list.List.Sort();
            return list;
        }

        // includes the lost connection
        bool IsConnected(string tag)
        {
            return CurrentConnections.FirstOrDefault(c => c.Tag.Equals(tag)) != default(ConnectionData) || LostConnections.FirstOrDefault(c => c.Tag.Equals(tag)) != default(ConnectionData);
        }
    }
}
