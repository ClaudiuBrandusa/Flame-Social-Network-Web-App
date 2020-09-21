using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models.Chat
{
    public class ConnectionData
    {
        public string ConnectionId { get; set; } // stored at runtime
        public string Tag { get; set; } // stored in database 
        public Queue<ChatRoom> CurrentChatRooms { get; set; } // current opened chatrooms
        // for now we are using the username as a tag
        public override bool Equals(object obj)
        {
            var other = obj as ConnectionData;
            return Tag.Equals(other.Tag);
        }

        public override int GetHashCode()
        {
            return Tag.GetHashCode();
        }
    }
}
