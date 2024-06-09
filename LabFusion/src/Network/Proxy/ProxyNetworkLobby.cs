namespace LabFusion.Network
{
    public class ProxyNetworkLobby : NetworkLobby
    {
        public LobbyMetadataInfo info;
        private readonly Dictionary<string, string> _cachedMetadata = new();

        /// <summary>
        /// Saves the metadata pair to the local dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void CacheMetadata(string key, string value)
        {
            _cachedMetadata[key] = value;
        }

        public override string GetMetadata(string key)
        {
            // With how the networking is setup, this is the best way to get it so there isn't a freeze between asking for the data and actually getting it
            return _cachedMetadata[key];
        }

        public override void SetMetadata(string key, string value)
        {
            _cachedMetadata[key] = value;
            SaveKey(key);

            var writer = ProxyNetworkLayer.NewWriter(FusionHelper.Network.MessageTypes.SetLobbyMetadata);
            writer.Put(key);
            writer.Put(value);
            ProxyNetworkLayer.Instance.SendToProxyServer(writer);
        }

        public override bool TryGetMetadata(string key, out string value)
        {
            return _cachedMetadata.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value);
        }

        public override Action CreateJoinDelegate(ulong lobbyId)
        {
            if (NetworkInfo.CurrentNetworkLayer is ProxyNetworkLayer proxyLayer)
            {
                return () => proxyLayer.JoinServer(lobbyId);
            }

            return null;
        }
    }
}
