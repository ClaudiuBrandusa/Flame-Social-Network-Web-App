using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models.Chat
{
    public class ContactsList
    {
        public ContactsList()
        {
            List = new List<Contact>();
        }
        public ContactsList(HashSet<ConnectionData> connections)
        {
            List = new List<Contact>();
            foreach(ConnectionData connection in connections)
            {
                List.Add(new Contact(connection));
            }
        }

        public List<Contact> List { get; set; }

        public void Add(HashSet<ConnectionData> connections)
        {
            foreach (ConnectionData connection in connections)
            {
                List.Add(new Contact(connection));
            }
        }
    }
}
