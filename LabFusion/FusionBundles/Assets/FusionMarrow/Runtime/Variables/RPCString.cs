#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Safety;
using LabFusion.Utilities;
using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else

#endif
    public class RPCString : RPCVariable
    {
#if MELONLOADER
        public RPCString(IntPtr intPtr) : base(intPtr) { }

        private string _latestValue = string.Empty;

        public string GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(string value)
        {
            string domain;
            if (isUrl(value) && !URLWhitelistManager.IsLinkWhitelisted(value, out domain))
            {
                FusionLogger.Warn($"Blocking sending the url {value}, as the domain {domain} is not whitelisted.");
                return false;
            }

            return RPCStringSender.SetValue(this, value);
        }

        public void ReceiveValue(string value)
        {
            // makes sure the text actually is a url before checking if its banned
            string domain;
            if (isUrl(value) && !URLWhitelistManager.IsLinkWhitelisted(value, out domain))
            {
                FusionLogger.Warn($"Received potentially dangerous URL. Blocking the url {value}, as the domain {domain} is not whitelisted.");
                return;
            }

            _latestValue = value;

            InvokeHolder();
        }

        private static bool isUrl(string value)
        {
            //checks text to see if its a valid url
            Uri uriResult;
            bool result = Uri.TryCreate(value, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        [HideFromIl2Cpp]
        public override void CatchupPlayer(PlayerID playerId) => RPCStringSender.CatchupValue(this, playerId);
#else
        public string GetLatestValue()
        {
            return string.Empty;
        }

        public bool SetValue(string value)
        {
            return false;
        }
#endif
    }
}