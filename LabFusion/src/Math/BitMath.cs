namespace LabFusion.Math;

public static class BitMath
{
    public static long MakeLong(int left, int right)
    {
        long value = left;
        value <<= 32;
        value |= (long)(uint)right;
        return value;
    }
}
