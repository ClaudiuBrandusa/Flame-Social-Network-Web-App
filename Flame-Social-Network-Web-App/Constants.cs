using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App
{
    public static class Constants
    {
        // Username
        public const int MinimumUsernameLength = 3;
        public const int MaximumUsernameLength = 30;

        // Password
        public const int MinimumPasswordLength = 6;
        public const int MaximumPasswordLength = 30;

        // Login Path
        public const string DefaultLoginPath = "/profile/login";

        // Connected Path
        public const string DefaultConnectedPath = "/";

        ///// Chat /////
        // Message length limit
        public const int MaxMessageLengthLimit = 10000000;

        // Waiting milliseconds time for reconnection
        public const int MaxReconnectWaitTime = 2000;

        // Max open chat rooms per client
        public const int MaxOpenChatRoomPerClient = 3;
    }
}
