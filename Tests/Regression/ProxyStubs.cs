namespace MelonLoader
{
    public static class MelonLogger
    {
        public static void Msg(string text) { }
        public static void Error(string text) { }
    }
}

namespace LabFusion
{
    public static class FusionMod { public static Version Version = new(1, 14, 1); }
}

namespace LabFusion.Utilities
{
    public static class TimeReferences { public static float DeltaTime = 20f; }
}

namespace LabFusion.UI.Popups
{
    public class Notification { public string Title, Message; public bool ShowPopup, SaveToMenu; }
    public static class Notifier { public static void Send(Notification notification) { } }
}

namespace LabFusion.Network
{
    public interface INetworkLobby { }
    public struct MatchmakerFilters { public static MatchmakerFilters Empty => default; }
    public class LobbyMetadataInfo { public bool HasLobbyOpen; }
    public static class LobbyMetadataSerializer
    {
        public static LobbyMetadataInfo ReadInfo(Proxy.ProxyNetworkLobby lobby) => new() { HasLobbyOpen = true };
    }
}

namespace LiteNetLib.Utils
{
    public class NetDataWriter
    {
        public void Put(ulong value) { }
    }
}

namespace LiteNetLib
{
    public class NetPacketReader
    {
        private readonly Queue<object> _values;
        public NetPacketReader(params object[] values) => _values = new(values);
        public uint GetUInt() => (uint)_values.Dequeue();
        public ulong GetULong() => (ulong)_values.Dequeue();
        public int GetInt() => (int)_values.Dequeue();
        public string GetString() => (string)_values.Dequeue();
    }
}

namespace LabFusion.Network.Proxy
{
    public enum MessageTypes { LobbyIDs, LobbyMetadata }
    public class ProxyNetworkLayer
    {
        public Action OnSend;
        public static LiteNetLib.Utils.NetDataWriter NewWriter(MessageTypes type) => new();
        public void SendToProxyServer(LiteNetLib.Utils.NetDataWriter writer) => OnSend?.Invoke();
    }
    public class ProxyLobbyRequestParameters
    {
        public LabFusion.Network.MatchmakerFilters Filters;
        public int VersionMajor, VersionMinor;
        public string LobbyCode;
        public void Put(LiteNetLib.Utils.NetDataWriter writer) { }
    }
    public class ProxyNetworkLobby : LabFusion.Network.INetworkLobby
    {
        public LabFusion.Network.LobbyMetadataInfo info;
        public void CacheMetadata(string key, string value) { }
    }
}
