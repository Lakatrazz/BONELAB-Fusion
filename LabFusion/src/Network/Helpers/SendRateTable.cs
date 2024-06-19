namespace LabFusion.Network;

public static class SendRateTable
{
    private static readonly int[] _objectTable = new int[14] {
        700,
        900,
        1000,
        1200,
        1400,
        1700,
        2000,
        2500,
        2700,
        3000,
        3200,
        3400,
        3700,
        4200,
    };

    public static int GetObjectSendRate(int bytesUp)
    {
        var tableLength = _objectTable.Length;

        for (var i = 0; i < tableLength; i++)
        {
            if (bytesUp <= _objectTable[i])
                return i + 1;
        }

        return tableLength + 1;
    }
}