using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models.Chat
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public List<ConnectionData> Connections { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>(); // Ordered by date time
    }
}
