using LabFusion.Utilities;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LabFusion.Network.Serialization;

public static class NetSerializerExtensions
{
    public static void SerializeValue(this INetSerializer serializer, ref object value)
    {
        bool hasValue = value != null;

        serializer.SerializeValue(ref hasValue);

        if (!hasValue)
        {
            return;
        }

        string typeName = null;

        if (!serializer.IsReader)
        {
            typeName = value.GetType().AssemblyQualifiedName;
        }

        serializer.SerializeValue(ref typeName);

        var type = Type.GetType(typeName);

        if (type.IsValueType)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(value);

            serializer.SerializeValue(ref json);

            value = JsonSerializer.Deserialize(json, type);
        }
        else if (type.IsAssignableTo(typeof(INetSerializable)))
        {
            var serializable = value as INetSerializable;

            serializer.SerializeValue(ref serializable, type);

            value = serializable;
        }
    }

    public static void SerializeValue<TSerializable>(this INetSerializer serializer, ref TSerializable value) where TSerializable : INetSerializable, new()
    {
        if (serializer.IsReader)
        {
            value = new TSerializable();
        }

        value.Serialize(serializer);
    }

    public static void SerializeValue(this INetSerializer serializer, ref INetSerializable value, Type type)
    {
        if (serializer.IsReader)
        {
            value = Activator.CreateInstance(type) as INetSerializable;
        }

        value.Serialize(serializer);
    }

    public static void SerializeValue(this INetSerializer serializer, ref Version value)
    {
        int major = 0, minor = 0, build = 0;

        if (!serializer.IsReader)
        {
            major = value.Major;
            minor = value.Minor;
            build = value.Build;
        }

        serializer.SerializeValue(ref major);
        serializer.SerializeValue(ref minor);
        serializer.SerializeValue(ref build);

        if (serializer.IsReader)
        {
            value = new Version(major, minor, build);
        }
    }

    public static void SerializeValue(this INetSerializer serializer, ref Dictionary<string, string> value)
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = value.Count;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            value = new(length);

            for (var i = 0; i < length; i++)
            {
                string keyString = null;
                string valueString = null;

                serializer.SerializeValue(ref keyString);
                serializer.SerializeValue(ref valueString);

                value.Add(keyString, valueString);
            }
        }
        else
        {
            foreach (var pair in value)
            {
                string keyString = pair.Key;
                string valueString = pair.Value;

                serializer.SerializeValue(ref keyString);
                serializer.SerializeValue(ref valueString);
            }
        }
    }

    public static void SerializeValue(this INetSerializer serializer, ref List<string> value)
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = value.Count;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            value = new(length);

            for (var i = 0; i < length; i++)
            {
                string readString = null;

                serializer.SerializeValue(ref readString);

                value[i] = readString;
            }
        }
        else
        {
            for (var i = 0; i < length; i++)
            {
                string writtenString = value[i];

                serializer.SerializeValue(ref writtenString);
            }
        }
    }
}
