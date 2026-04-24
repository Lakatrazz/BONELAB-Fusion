using System.Collections;
using System.Globalization;
using Epic.OnlineServices;
using LabFusion.Utilities;
using Steamworks;

namespace LabFusion.Network.EpicGames;

internal class EOSSteamAuth : EOSAuthInterface
{
    private const uint appID = 1592190;
    private const string authTicketIdentity = "epiconlineservices";
    
    internal override ExternalAccountType AccountType => ExternalAccountType.Steam;
    
    internal override ExternalCredentialType CredentialType => ExternalCredentialType.SteamSessionTicket;

    internal override IEnumerator GetLoginTicketAsync(Action<string> onTokenReceived)
    {
        if (GameHasSteamworks())
            ShutdownGameClient();
        
        // Async callbacks seem to work. ¯\_(ツ)_/¯
        if (!SteamClient.IsValid)
            SteamClient.Init(appID);
        
        var previousCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        
        var ticketTask = SteamUser.GetAuthTicketForWebApiAsync(authTicketIdentity);
        while (!ticketTask.IsCompleted)
            yield return null;
        
        Thread.CurrentThread.CurrentCulture = previousCulture;
        
        if (!ticketTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException("getting steam token", ticketTask.Exception);
            onTokenReceived?.Invoke(null);
            yield break;
        }
        if (ticketTask.IsFaulted)
        {
            FusionLogger.LogException("getting steam token", ticketTask.Exception);
            onTokenReceived?.Invoke(null);
            yield break;
        }

        var ticket = ticketTask.Result;
        string hexTicket = BitConverter.ToString(ticket.Data).Replace("-", "");
        
        onTokenReceived?.Invoke(hexTicket);
    }
    
    internal override IEnumerator GetDisplayNameAsync(Action<string> onDisplayNameReceived)
    {
        string displayName = SteamClient.Name;
        onDisplayNameReceived?.Invoke(displayName);
        yield break;
    }
    
    private const string STEAMWORKS_ASSEMBLY_NAME = "Il2CppFacepunch.Steamworks.Win64";
    
    private static bool GameHasSteamworks()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (assembly.FullName != null && assembly.FullName.StartsWith(STEAMWORKS_ASSEMBLY_NAME))
            {
                return true;
            }
        }

        return false;
    }
    
    private static void ShutdownGameClient()
    {
        Il2CppSteamworks.SteamClient.Shutdown();
    }
    
    internal override void OnShutdown()
    {
        SteamClient.Shutdown();
    }
}