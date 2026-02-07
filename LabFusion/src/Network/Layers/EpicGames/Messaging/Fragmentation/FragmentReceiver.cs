using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles receiving and reassembling fragmented messages. 
/// </summary>
internal class FragmentReceiver
{
    private const int MaxFragments = 1000;
    private const int CleanupIntervalSeconds = 30;

    private readonly Dictionary<(string SenderId, ushort FragmentId), FragmentCollection> _pendingFragments = new();
    private readonly object _lock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;

    public bool TryHandleFragment(
        byte[] buffer,
        int bytesWritten,
        string senderId,
        out byte[] reassembledData)
    {
        reassembledData = null;

        if (bytesWritten < FragmentHeader.Size)
            return false;

        var (fragmentId, fragmentIndex, totalFragments) = FragmentHeader.Read(buffer.AsSpan());

        if (!ValidateHeader(totalFragments, fragmentIndex))
            return false;

        int fragmentDataSize = bytesWritten - FragmentHeader.Size;
        var key = (senderId, fragmentId);

        lock (_lock)
        {
            if (!_pendingFragments.TryGetValue(key, out var collection))
            {
                collection = FragmentCollection.Create(totalFragments);
            }

            // Ignore duplicates
            if (collection.ReceivedFlags[fragmentIndex])
                return false;

            // Store fragment
            StoreFragment(ref collection, buffer, fragmentIndex, fragmentDataSize);
            _pendingFragments[key] = collection;

            // Check if complete
            if (!collection.IsComplete(totalFragments))
                return false;

            reassembledData = collection.Reassemble();
            _pendingFragments.Remove(key);
            return true;
        }
    }

    public void CleanupIfNeeded()
    {
        if ((DateTime.UtcNow - _lastCleanup).TotalSeconds < CleanupIntervalSeconds)
            return;

        CleanupStaleFragments();
        _lastCleanup = DateTime.UtcNow;
    }

    private static bool ValidateHeader(ushort totalFragments, ushort fragmentIndex)
    {
        return totalFragments > 0 &&
               totalFragments <= MaxFragments &&
               fragmentIndex < totalFragments;
    }

    private static void StoreFragment(
        ref FragmentCollection collection,
        byte[] buffer,
        int fragmentIndex,
        int fragmentDataSize)
    {
        collection.Fragments[fragmentIndex] = new byte[fragmentDataSize];
        Array.Copy(buffer, FragmentHeader.Size, collection.Fragments[fragmentIndex], 0, fragmentDataSize);
        collection.ReceivedFlags[fragmentIndex] = true;
        collection.ReceivedCount++;
        collection.TotalSize += fragmentDataSize;
        collection.LastReceived = DateTime.UtcNow;
    }

    private void CleanupStaleFragments()
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-CleanupIntervalSeconds);

        lock (_lock)
        {
            var keysToRemove = _pendingFragments
                .Where(kvp => kvp.Value.LastReceived < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _pendingFragments.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
#if DEBUG
                FusionLogger.Log($"Cleaned up {keysToRemove.Count} stale fragment collections");
#endif
            }
        }
    }
}