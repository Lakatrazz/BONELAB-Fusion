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
            Console.WriteLine("command: " + command);
        }
    }
});

tickThread.Start();
commandThread.Start();
