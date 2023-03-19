using FusionHelper.Network;
using FusionHelper.Steamworks;
using FusionHelper.WebSocket;
using Steamworks;
using Steamworks.Data;

SteamHandler.Init();
NetworkHandler.Init();

Thread tickThread = new(() =>
{
    while (true)
    {
        NetworkHandler.PollEvents();
        SteamHandler.Tick();
        // Throttle a little bit to not burn 100% CPU
        Thread.Sleep(8);
    }
});

Thread commandThread = new(() =>
{
    while (true)
    {
        string? command = Console.ReadLine();
        if (command != null)
        {
            //Console.WriteLine("command: " + command);
            switch (command)
            {
                case string s when s.StartsWith("connect"):
                    ulong serverId = ulong.Parse(command.Split(' ')[1]);
                    Console.WriteLine("Attempting server connection to " + serverId);
                    NetworkHandler.SendToClient(BitConverter.GetBytes(serverId), MessageTypes.JoinServer);
                    break;
                case "ping":
                    double curTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    NetworkHandler.SendToClient(BitConverter.GetBytes(curTime), MessageTypes.Ping);
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
    }
});

tickThread.Start();
commandThread.Start();
