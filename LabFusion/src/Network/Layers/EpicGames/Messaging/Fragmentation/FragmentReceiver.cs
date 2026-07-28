using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles receiving and reassembling fragmented messages. 
/// </summary>
internal class FragmentReceiver
{
    private const int MaxFragments = 1000;
    private const int CleanupIntervalSeconds = 30;
    private const int MaxPendingCollections = 64;
    private const int MaxAssemblyTimeSeconds = 60;

    private readonly Dictionary<(string SenderId, ushort FragmentId), FragmentCollection> _pendingFragments = new();
    private readonly object _lock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;

    internal bool TryHandleFragment(
        byte[] buffer,
        int bytesWritten,
        string senderId,
        out byte[] reassembledData)
    {
        reassembledData = null;

        if (bytesWritten < FragmentHeader.Size)
            return false;

        var (fragmentId, fragmentIndex, totalFragments) = FragmentHeader.Read(buffer.AsSpan(0, bytesWritten));

        if (!ValidateHeader(totalFragments, fragmentIndex))
            return false;

        int fragmentDataSize = bytesWritten - FragmentHeader.Size;
        var key = (senderId, fragmentId);

        lock (_lock)
        {
            if (!_pendingFragments.TryGetValue(key, out var collection))
            {
                if (_pendingFragments.Count >= MaxPendingCollections)
                    return false;

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

    internal void CleanupIfNeeded()
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
        var idleCutoff = DateTime.UtcNow.AddSeconds(-CleanupIntervalSeconds);
        var assemblyCutoff = DateTime.UtcNow.AddSeconds(-MaxAssemblyTimeSeconds);

        lock (_lock)
        {
            var keysToRemove = _pendingFragments
                .Where(kvp => kvp.Value.LastReceived < idleCutoff || kvp.Value.FirstReceived < assemblyCutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _pendingFragments.Remove(key);
            }
        }
    }
}