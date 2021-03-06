﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models.Chat
{
    public class Message
    {
        public string Sender { get; set; }
        [JsonIgnore]
        public int ChatRoomId { get; set; }
        [StringLength(Constants.MaxMessageLengthLimit, ErrorMessage = "The message length should not be longer than {1} characters")]
        public string Content { get; set; }

        public DateTime Time { get; set; }
    }
}
