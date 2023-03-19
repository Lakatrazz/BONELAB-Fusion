using FusionHelper.Steamworks;
using FusionHelper.WebSocket;

SteamHandler.Init();
NetworkHandler.Init();

Thread tickThread = new(() =>
{
    while (true)
    {
        NetworkHandler.PollEvents();
        SteamHandler.Tick();
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
                    NetworkHandler.SendToClient(BitConverter.GetBytes(serverId), FusionHelper.Network.MessageTypes.JoinServer);
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
