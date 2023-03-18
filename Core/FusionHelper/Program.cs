using FusionHelper.Steamworks;
using FusionHelper.WebSocket;
using Ruffles.Core;

SteamHandler.Init();
NetworkHandler.Init();

while (true)
{
    NetworkHandler.PollEvents();
    // TODO: commands
}

//Console.ReadLine();
