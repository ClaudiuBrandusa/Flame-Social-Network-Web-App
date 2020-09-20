using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models.Chat
{
    public class Contact
    {
        public Contact(ConnectionData connection)
        {
            Tag = connection.Tag;
        }

        public string Tag { get; set; }
    }
}
