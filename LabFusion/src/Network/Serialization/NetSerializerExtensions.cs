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
            value = null;
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
            int casted = serializer.IsReader ? default : (int)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(uint))
        {
            uint casted = serializer.IsReader ? default : (uint)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(short))
        {
            short casted = serializer.IsReader ? default : (short)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ushort))
        {
            ushort casted = serializer.IsReader ? default : (ushort)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(long))
        {
            long casted = serializer.IsReader ? default : (long)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ulong))
        {
            ulong casted = serializer.IsReader ? default : (ulong)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(double))
        {
            double casted = serializer.IsReader ? default : (double)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(bool))
        {
            bool casted = serializer.IsReader ? default : (bool)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte))
        {
            byte casted = serializer.IsReader ? default : (byte)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(sbyte))
        {
            sbyte casted = serializer.IsReader ? default : (sbyte)value;
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
            if (serializer is NetReader reader)
            {
                reader.ValidateCollectionLength(length, sizeof(int) * 2);
            }

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
            if (serializer is NetReader reader)
            {
                reader.ValidateCollectionLength(length, sizeof(int));
            }

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
