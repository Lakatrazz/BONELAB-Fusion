using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruffles.Channeling;
using Ruffles.Configuration;
using Ruffles.Connections;
using Ruffles.Core;

namespace FusionHelper.WebSocket
{
    internal static class NetworkHandler
    {
        private static RuffleSocket server;

        public static void Init()
        {
            server = new RuffleSocket(new SocketConfig()
            {
                ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
                ChannelTypes = new ChannelType[]
                {
                    ChannelType.Reliable,
                    ChannelType.Unreliable,
                },
                DualListenPort = 9000,
            });
            server.Start();

            Console.WriteLine("Initialized UDP socket at localhost:9000");
        }
    }
}
