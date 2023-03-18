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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static RuffleSocket Server { get; private set; }
        public static Connection ClientConnection { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void Init()
        {
            Server = new RuffleSocket(new SocketConfig()
            {
                ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
                ChannelTypes = new ChannelType[]
                {
                    ChannelType.Reliable,
                    ChannelType.Unreliable,
                },
                DualListenPort = 9000,
            });
            Server.Start();

            Console.WriteLine("Initialized UDP socket at localhost:9000");
        }

        public static void PollEvents()
        {
            NetworkEvent serverEvent = Server.Poll();

            if (serverEvent.Type != NetworkEventType.Nothing)
            {
                Console.WriteLine("ServerEvent: " + serverEvent.Type);

                if (serverEvent.Type == NetworkEventType.Connect)
                {
                    ClientConnection = serverEvent.Connection;
                    Console.WriteLine("Client was connected");
                }

                if (serverEvent.Type == NetworkEventType.Data)
                {
                    Console.WriteLine("Got message: \"" + Encoding.ASCII.GetString(serverEvent.Data.Array, serverEvent.Data.Offset, serverEvent.Data.Count) + "\"");
                }
            }

            serverEvent.Recycle();
        }
    }
}
