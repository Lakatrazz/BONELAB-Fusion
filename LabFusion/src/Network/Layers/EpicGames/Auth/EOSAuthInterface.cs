using Epic.OnlineServices;
using System.Collections;

namespace LabFusion.Network.EpicGames;

internal abstract class EOSAuthInterface
{
    internal abstract ExternalAccountType AccountType { get; }
    
    internal abstract ExternalCredentialType CredentialType { get; }
    
    /// <summary>
    /// Indicates whether this interface can return a null authentication token.
    /// </summary>
    internal virtual bool AllowNullToken => false;
    
    /// <summary>
    /// Indicates whether DisplayName should be passed into UserLoginInfo.
    /// </summary>
    internal virtual bool LoginWithDisplayName => false;

    internal abstract IEnumerator GetLoginTicketAsync(Action<string> onTokenReceived);
    internal abstract IEnumerator GetDisplayNameAsync(Action<string> onDisplayNameReceived);
    internal virtual void OnShutdown() { }
}