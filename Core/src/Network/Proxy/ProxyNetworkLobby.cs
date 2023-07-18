using System;
using System.Collections.Generic;

namespace LabFusion.Network
{
    public class ProxyNetworkLobby : INetworkLobby
    {
        public LobbyMetadataInfo info;
        private Dictionary<string, string> _cachedMetadata = new();

        public string GetMetadata(string key)
        {
            // With how the networking is setup, this is the best way to get it so there isn't a freeze between asking for the data and actually getting it
            return _cachedMetadata[key];
        }

        public void SetMetadata(string key, string value)
        {
            _cachedMetadata[key] = value;
            var writer = ProxyNetworkLayer.NewWriter(FusionHelper.Network.MessageTypes.SetLobbyMetadata);
            writer.Put(key);
            writer.Put(value);
            ProxyNetworkLayer.Instance.SendToProxyServer(writer);
        }

        public bool TryGetMetadata(string key, out string value)
        {
            value = _cachedMetadata[key];
            return string.IsNullOrWhiteSpace(value);
        }

        public Action CreateJoinDelegate(LobbyMetadataInfo info)
        {
            if (NetworkInfo.CurrentNetworkLayer is ProxyNetworkLayer proxyLayer)
            {
                return () => proxyLayer.JoinServer(info.LobbyId);
            }

            return null;
        }
    }
}
