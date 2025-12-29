namespace LabFusion.Network;

public static class NetworkConnectionManager
{
    private class DisconnectTimeout
    {
        public ulong PlatformID;

        public float TimeRemaining;
    }

    private static readonly List<DisconnectTimeout> _disconnectTimeouts = new();

    /// <summary>
    /// Ensures that a user is disconnected after a given duration.
    /// </summary>
    /// <param name="platformID"></param>
    /// <param name="duration"></param>
    public static void TimeoutDisconnect(ulong platformID, float duration = 1f)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        _disconnectTimeouts.Add(new DisconnectTimeout()
        {
            PlatformID = platformID,
            TimeRemaining = duration,
        });
    }

    /// <summary>
    /// Forcefully closes the connection for a connected user.
    /// </summary>
    /// <param name="platformID"></param>
    public static void DisconnectUser(ulong platformID)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var layer = NetworkInfo.Layer;

        layer.DisconnectUser(platformID);
    }

    internal static void OnUpdate(float deltaTime)
    {
        ProcessDisconnectTimeouts(deltaTime);
    }

    private static void ProcessDisconnectTimeouts(float deltaTime)
    {
        for (var i = _disconnectTimeouts.Count - 1; i >= 0; i--)
        {
            var timeout = _disconnectTimeouts[i];

            timeout.TimeRemaining -= deltaTime;

            if (timeout.TimeRemaining <= 0f)
            {
                DisconnectUser(timeout.PlatformID);

                _disconnectTimeouts.RemoveAt(i);
                continue;
            }
        }
    }
}
