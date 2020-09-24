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
        
        public static HashSet<ChatRoom> ChatRooms = new HashSet<ChatRoom>(); // Every active chatroom goes here
        static int chatRoomId = 0;
        public override async Task OnConnectedAsync()
        {
            var connection = new ConnectionData { ConnectionId = Context.ConnectionId, Tag = Context.User.Identity.Name }; //Context.User.Identity.Name };

            CurrentConnections.Add(connection);
            if(LostConnections.Contains(connection))
            {
                // then it's a lost connection
                LostConnections.Remove(connection);
            }
            connection.CurrentChatRooms = new Queue<ChatRoom>();
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
                return Task.CompletedTask;
            }
            if(!IsConnected(tag))
            {
                // then we can't talk
            }
            ConnectionData user = GetConnectionDataByTag(Context.User.Identity.Name);
            if(user == null)
            {
                // then something went wrong
                return Task.CompletedTask;
            }
            // if we have a chatroom already open with tag
            if(user.CurrentChatRooms.Count > 0 && user.CurrentChatRooms.Any(x => x.Connections.Any(c => c.Tag.Equals(tag))))
            {
                // then we deque and enque the room
                ChatRoom tmp = default;
                // while we do not have a connection with tag
                while(!user.CurrentChatRooms.Peek().Connections.Any(c => c.Tag.Equals(tag)))
                {
                    tmp = user.CurrentChatRooms.Dequeue();
                    user.CurrentChatRooms.Enqueue(tmp);
                }
                // then we found the chatroom
                tmp = user.CurrentChatRooms.Dequeue();
                user.CurrentChatRooms.Enqueue(tmp);
                Clients.Caller.SendAsync("closeChatRoom", tmp.Id);
                Clients.Caller.SendAsync("openChatRoomWith", tag, tmp.Id);
                return Task.CompletedTask;
            }
            if(GetConnectionDataByTag(Context.User.Identity.Name).CurrentChatRooms.Count >= Constants.MaxOpenChatRoomPerClient)
            {
                // we can't have too many chat rooms open
                // so then we will close the last one used
                ChatRoom room = user.CurrentChatRooms.Dequeue();
                Clients.Caller.SendAsync("closeChatRoom", room.Id);
                return Task.CompletedTask;
            }
            ChatRoom chatRoom = GetChatRoomByTags(tag, user.Tag);
            // if this is a brand new conversation
            if(chatRoom == null || chatRoom == default)
            { // then we create another chat room
                chatRoom = new ChatRoom();
                chatRoom.Id = GenerateChatRoomId();
                chatRoom.Connections = new List<ConnectionData>();
                chatRoom.Connections.Add(user);
                ConnectionData receiver = GetConnectionDataByTag(tag);
                chatRoom.Connections.Add(receiver);
                if(receiver.CurrentChatRooms.Any(r => r.Id != chatRoom.Id))
                {
                    receiver.CurrentChatRooms.Enqueue(chatRoom);
                    if(receiver.CurrentChatRooms.Count > Constants.MaxOpenChatRoomPerClient)
                    {
                        Clients.Caller.SendAsync("closeChatRoom", receiver.CurrentChatRooms.Dequeue().Id);
                    }
                }
                ChatRooms.Add(chatRoom);
                user.CurrentChatRooms.Enqueue(chatRoom);
                Clients.Caller.SendAsync("openChatRoomWith", tag, chatRoom.Id);
                return Task.CompletedTask;
            } // otherwise we are good to go

            user.CurrentChatRooms.Enqueue(chatRoom);
            Clients.Caller.SendAsync("openChatRoomWith", tag, chatRoom.Id);
            // here we should load the history
            return Task.CompletedTask;
        }

        /*public Task EndChatWith(string tag) //deprecated
        {
            Clients.Caller.SendAsync("alert", "deprecated method used!");
            return Task.CompletedTask;
            // tag validation
            if (Context.User.Identity.Name.Equals(tag))
            {
                // we can't chat with ourselves
                return Task.CompletedTask;
            }
            if (!IsConnected(tag))
            {
                // then we can't talk
            }
            ConnectionData user = GetConnectionDataByTag(Context.User.Identity.Name);
            if(user.CurrentChatRooms.Count > 0 && user.CurrentChatRooms.Any(r => r.Connections.Any(c => c.Tag.Equals(tag))))
            {
                // then we deque and enque the room
                ChatRoom tmp = default;
                // while we do not have a connection with tag
                while (!user.CurrentChatRooms.Peek().Connections.Any(c => c.Tag.Equals(tag)))
                {
                    tmp = user.CurrentChatRooms.Dequeue();
                    user.CurrentChatRooms.Enqueue(tmp);
                }
                // then we found the chatroom
                tmp = user.CurrentChatRooms.Dequeue();
                //ChatRooms.Remove(tmp);
                return Clients.Caller.SendAsync("closeChatRoom", tmp.Id);
            }
            return Task.CompletedTask;
        }*/

        public Task EndChat(int chatRoomId)
        {
            if (!IsConnected(Context.User.Identity.Name))
            {
                // then something went wrong
                Clients.Caller.SendAsync("alert", "the user that you want to end chat with is not connected");
                return Task.CompletedTask;
            }
            ConnectionData user = GetConnectionDataByTag(Context.User.Identity.Name);
            // we check if we have this chat room open
            if(user.CurrentChatRooms.Count > 0 && user.CurrentChatRooms.Any(r => r.Id == chatRoomId))
            {
                // we know that we have this chat room open
                // so we have to find it
                ChatRoom tmp = default;
                while(user.CurrentChatRooms.Peek().Id != chatRoomId)
                {
                    tmp = user.CurrentChatRooms.Dequeue();
                    user.CurrentChatRooms.Enqueue(tmp);
                }
                // now that we have found the chat room we can close it
                tmp = user.CurrentChatRooms.Dequeue();
                Clients.Caller.SendAsync("closeChatRoom", chatRoomId);
                return Task.CompletedTask;
            } // else we have nothing to close
            return Task.CompletedTask;
        }

        public Task SendMessageTo(string message, int chatRoomId)
        {
            // user validation
            if (!IsConnected(Context.User.Identity.Name))
            {
                // then we can't talk
                return Task.CompletedTask;
            }
            // message validation
            if(string.IsNullOrEmpty(message))
            {
                return Task.CompletedTask;
            }
            ConnectionData user = GetConnectionDataByTag(Context.User.Identity.Name);
            // we check if we already have the chat room open
            if (user.CurrentChatRooms.Count > 0 && user.CurrentChatRooms.Any(r => r.Id == chatRoomId))
            {
                // then we deque and enque the room
                ChatRoom tmp = default;
                // while we do not have a connection with tag
                while (user.CurrentChatRooms.Peek().Id != chatRoomId)
                {
                    tmp = user.CurrentChatRooms.Dequeue();
                    user.CurrentChatRooms.Enqueue(tmp);
                }
                // then we found the chatroom
                tmp = user.CurrentChatRooms.Dequeue();
                user.CurrentChatRooms.Enqueue(tmp);
                //Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessageIn", tmp.Id, message);
                if(tmp.Connections.Count == 2)
                {
                    ConnectionData receiver = GetChatRoomReceiver(user.Tag, tmp);
                    if (receiver == null)
                    {
                        Clients.Caller.SendAsync("alert", "null");
                    }
                    else if(receiver == default)
                    {
                        Clients.Caller.SendAsync("alert", "default");
                    }
                    // we check if the receiver does not have a reference to the current chat room
                    if (receiver.CurrentChatRooms.Count == 0 || receiver.CurrentChatRooms.Any(r => r.Id != tmp.Id))
                    {
                        if (receiver.CurrentChatRooms.Count >= Constants.MaxOpenChatRoomPerClient)
                        {
                            Clients.Caller.SendAsync("closeChatRoom", receiver.CurrentChatRooms.Dequeue().Id);
                        }
                        // then we have to add the reference
                        receiver.CurrentChatRooms.Enqueue(tmp);
                    }
                    Clients.Client(receiver.ConnectionId).SendAsync("openChatRoomWith", user.Tag, tmp.Id);
                    Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessageIn", tmp.Id, message, user.Tag);
                }else if(tmp.Connections.Count == 1)
                {

                    // then we have nothing to do here
                }else
                {
                    SendMessageInChatRoom(tmp, message, user.Tag);
                }
                AddMessageToChatRoom(tmp, message, user);
                return Clients.Caller.SendAsync("SendMessageIn", tmp.Id, message);
            } // else we search the chatroom from the list
            ChatRoom chatRoom = GetChatRoomById(chatRoomId);
            if(chatRoom == null || chatRoom == default)
            {
                Clients.Caller.SendAsync("alert", "wrong chat room id");
                // then it means that something went wrong and we got a wrong chat room id
                return Task.CompletedTask;
            } // otherwise we found the chat room
            user.CurrentChatRooms.Enqueue(chatRoom);
            if(user.CurrentChatRooms.Count > Constants.MaxOpenChatRoomPerClient)
            {
                Clients.Caller.SendAsync("closeChatRoom", user.CurrentChatRooms.Dequeue().Id);
            }
            Clients.Caller.SendAsync("openChatRoomWith", user.Tag, chatRoom.Id);
            Clients.Caller.SendAsync("SendMessageIn", chatRoom.Id, message);
            // adding the message to the chatroom's history
            AddMessageToChatRoom(chatRoom, message, user);
            Clients.Client(GetChatRoomReceiver(user.Tag, chatRoom).ConnectionId).SendAsync("ReceiveMessageIn", chatRoom.Id, message);
            SendMessageInChatRoom(chatRoom, message, user.Tag);
            return Task.CompletedTask;
        }

        public Task OpenChatRoom(int chatRoomId) // Deprecated for now
        {
            // firstly we validate the caller
            if(!IsConnected(Context.User.Identity.Name))
            {
                // then it means that something went wrong
                // like the user disconnected
                // or the user has a slow internet speed
                return Task.CompletedTask;
            }
            ConnectionData user = GetConnectionDataByTag(Context.User.Identity.Name);
            // then we check if we already have opened the chat room
            if(user.CurrentChatRooms.Count > 0 && user.CurrentChatRooms.Any(r => r.Id == chatRoomId))
            {
                // then we have to open this room first
                ChatRoom tmp = default;
                // while we do not have a connection with tag
                while (user.CurrentChatRooms.Peek().Id != chatRoomId)
                {
                    tmp = user.CurrentChatRooms.Dequeue();
                    user.CurrentChatRooms.Enqueue(tmp);
                }
                // then we found the chatroom
                tmp = user.CurrentChatRooms.Dequeue();
                user.CurrentChatRooms.Enqueue(tmp);
                // for two users conversation
                if(tmp.Connections.Count == 2)
                {
                    Clients.Caller.SendAsync("openChatRoomWith", GetChatRoomReceiver(user.Tag, tmp).Tag, chatRoomId);
                }else
                {
                    Clients.Caller.SendAsync("openChatRoomWith", GetChatRoomReceivers(user.Tag, tmp), chatRoomId);
                }
                return Task.CompletedTask;
            } // otherwise
            ChatRoom chatRoom = GetChatRoomById(chatRoomId);
            // we validate the result
            if(chatRoom == null || chatRoom == default)
            {
                // then it means that we got a wrong id
                return Task.CompletedTask;
            }
            // else we found the searched chat room
            // but we check if we have reached the chat room limit
            if(user.CurrentChatRooms.Count >= Constants.MaxOpenChatRoomPerClient)
            {
                // then we remove the last used chat room from the queue
                user.CurrentChatRooms.Dequeue();
            } // else we just enqueue the chat room
            user.CurrentChatRooms.Enqueue(chatRoom);
            return Task.CompletedTask;
        }

        ///////////// Tools /////////////
        ConnectionData GetConnectionDataByTag(string tag)
        {
            return CurrentConnections.FirstOrDefault(c => c.Tag.Equals(tag));
        }

        static ConnectionData GetChatRoomReceiver(string tag, ChatRoom chatRoom)
        {
            return chatRoom.Connections.FirstOrDefault(c => !c.Tag.Equals(tag));
        }

        static string GetChatRoomReceivers(string tag, ChatRoom chatRoom)
        {
            string s = "";
            foreach(ConnectionData connection in chatRoom.Connections)
            {
                if(!connection.Tag.Equals(tag))
                {
                    s += connection.Tag + ", ";
                }
            }
            if(s.Length > 0)
            {
                s = s.Substring(0, s.Length - 2); // we have to trim the last `, ` inserted in the loop from above
            }
            return s;
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

        public static int GenerateChatRoomId()
        {
            return chatRoomId++;
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

        public static ChatRoom GetChatRoomByTags(string tag1, string tag2)
        {
            return ChatRooms.FirstOrDefault(r => r.Connections.Any(c => c.Tag.Equals(tag1)) && r.Connections.Any(c => c.Tag.Equals(tag2)));
        }

        public static ChatRoom GetChatRoomById(int id)
        {
            return ChatRooms.FirstOrDefault(r => r.Id == id);
        }

        // includes the lost connection
        bool IsConnected(string tag)
        {
            return CurrentConnections.FirstOrDefault(c => c.Tag.Equals(tag)) != default(ConnectionData) || LostConnections.FirstOrDefault(c => c.Tag.Equals(tag)) != default(ConnectionData);
        }

        Task SendMessageInChatRoom(ChatRoom chatRoom, string message, string senderTag)
        {
            // this method shall be used only for conversations between more than 2 users
            if(chatRoom.Connections.Count < 3)
            {
                return Task.CompletedTask;
            }
            foreach(ConnectionData connection in chatRoom.Connections)
            {
                if(!connection.Tag.Equals(senderTag))
                {
                    if(connection.CurrentChatRooms.Count >= Constants.MaxOpenChatRoomPerClient)
                    {
                        // if no one of the chat rooms is chatRoom
                        if(connection.CurrentChatRooms.Any(c => c.Id != chatRoom.Id))
                        {
                            // then we close the last used chat room
                            Clients.Caller.SendAsync("closeChatRoom", connection.CurrentChatRooms.Dequeue().Id);
                            // and we open chatRoom
                            Clients.Client(connection.ConnectionId).SendAsync("openChatRoomWith", GetChatRoomReceivers(connection.Tag, chatRoom), chatRoom.Id);
                        }
                    }else // otherwise we just open chatRoom
                    {
                        Clients.Client(connection.ConnectionId).SendAsync("openChatRoomWith", GetChatRoomReceivers(connection.Tag, chatRoom), chatRoom.Id);
                    }
                    
                    Clients.Client(connection.ConnectionId).SendAsync("ReceiveMessageIn", chatRoom.Id, message, senderTag);
                }
            }
            return Task.CompletedTask;
        }

        static void AddMessageToChatRoom(ChatRoom chatRoom, string message, ConnectionData sender)
        {
            chatRoom.Messages.Add(new Message() { ChatRoomId = chatRoom.Id, Content = message, Sender = sender.Tag, Time = DateTime.Now });
        }
    }
}
