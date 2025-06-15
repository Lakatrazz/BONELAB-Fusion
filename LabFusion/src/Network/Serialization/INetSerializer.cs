namespace LabFusion.Network.Serialization;

public interface INetSerializer
{
    public bool IsReader { get; }

    void SerializeValue(ref byte value);

    void SerializeValue(ref bool value);

    void SerializeValue(ref double value);

    void SerializeValue(ref short value);

    void SerializeValue(ref int value);

    void SerializeValue(ref long value);

    void SerializeValue(ref sbyte value);

    void SerializeValue(ref float value);

    void SerializeValue(ref ushort value);

    void SerializeValue(ref uint value);

    void SerializeValue(ref ulong value);

    void SerializeValue(ref string value);

    void SerializeValue(ref byte[] value);

    void SerializeValue(ref ArraySegment<byte> value);

    void SerializeValue(ref string[] value);

    void SerializeValue<TEnum>(ref TEnum value) where TEnum : struct, Enum;

    void SerializeValue<TEnum>(ref TEnum value, Precision precision) where TEnum : struct, Enum;

    void SerializeValue(ref byte? value);

    void SerializeValue(ref int? value);

    void SerializeValue(ref ushort? value);
}
