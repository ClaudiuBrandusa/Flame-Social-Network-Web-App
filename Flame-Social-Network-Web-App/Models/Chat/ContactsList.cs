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

        public List<Contact> List { get; set; }
    }
}
