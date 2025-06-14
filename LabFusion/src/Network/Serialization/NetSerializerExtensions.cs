using System.Text.Json;

namespace LabFusion.Network.Serialization;

public static class NetSerializerExtensions
{
    public static void SerializeValueByJson(this INetSerializer serializer, ref object value)
    {
        if (!SerializeNullable(serializer, ref value))
        {
            return;
        }

        var type = SerializeType(serializer, ref value);

        SerializeJson(serializer, ref value, type);
    }

    private static Type SerializeType(INetSerializer serializer, ref object value)
    {
        string typeName = null;

        if (!serializer.IsReader)
        {
            typeName = value.GetType().AssemblyQualifiedName;
        }

        serializer.SerializeValue(ref typeName);

        var type = Type.GetType(typeName);

        return type;
    }

    private static bool SerializeNullable(INetSerializer serializer, ref object value)
    {
        bool hasValue = value != null;

        serializer.SerializeValue(ref hasValue);

        if (!hasValue)
        {
            return false;
        }

        return true;
    }

    private static void SerializeJson(INetSerializer serializer, ref object value, Type type)
    {
        var options = new JsonSerializerOptions()
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        var json = JsonSerializer.SerializeToUtf8Bytes(value, options);

        serializer.SerializeValue(ref json);

        if (serializer.IsReader)
        {
            value = JsonSerializer.Deserialize(json, type, options);
        }
    }

    public static void SerializeValue(this INetSerializer serializer, ref object value)
    {
        if (!SerializeNullable(serializer, ref value))
        { 
            return; 
        }

        var type = SerializeType(serializer, ref value);

        if (type == typeof(int))
        {
            int casted = (int)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(uint))
        {
            uint casted = (uint)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(short))
        {
            short casted = (short)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ushort))
        {
            ushort casted = (ushort)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(long))
        {
            long casted = (long)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ulong))
        {
            ulong casted = (ulong)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(double))
        {
            double casted = (double)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(bool))
        {
            bool casted = (bool)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte))
        {
            byte casted = (byte)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(sbyte))
        {
            sbyte casted = (sbyte)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte?))
        {
            byte? casted = value as byte?;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ushort?))
        {
            ushort? casted = value as ushort?;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte[]))
        {
            byte[] casted = value as byte[];
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(string))
        {
            string casted = value as string;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type.IsAssignableTo(typeof(INetSerializable)))
        {
            var serializable = value as INetSerializable;

            serializer.SerializeValue(ref serializable, type);

            if (serializer.IsReader)
            {
                value = serializable;
            }
        }
        else if (type.IsValueType || type.IsSerializable)
        {
            SerializeJson(serializer, ref value, type);
        }
        else
        {
            throw new NotSupportedException($"Serialization of type {type.FullName} is not supported.");
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

                value.Add(readString);
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
