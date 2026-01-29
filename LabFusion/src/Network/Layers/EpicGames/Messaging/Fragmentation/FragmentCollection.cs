namespace LabFusion.Network.EpicGames;

/// <summary>
/// Holds fragments of a single message being reassembled.
/// </summary>
internal struct FragmentCollection
{
    public byte[][] Fragments;
    public bool[] ReceivedFlags;
    public int ReceivedCount;
    public int TotalSize;
    public DateTime LastReceived;

    public static FragmentCollection Create(int totalFragments)
    {
        return new FragmentCollection
        {
            Fragments = new byte[totalFragments][],
            ReceivedFlags = new bool[totalFragments],
            ReceivedCount = 0,
            TotalSize = 0,
            LastReceived = DateTime.UtcNow
        };
    }

    public bool IsComplete(int expectedFragments)
    {
        return ReceivedCount >= expectedFragments;
    }

    public byte[] Reassemble()
    {
        var result = new byte[TotalSize];
        int offset = 0;

        foreach (var fragment in Fragments)
        {
            if (fragment == null) continue;
            Array.Copy(fragment, 0, result, offset, fragment.Length);
            offset += fragment.Length;
        }

        return result;
    }
}