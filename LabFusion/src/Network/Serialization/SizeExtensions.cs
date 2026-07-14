using System.Text;

namespace LabFusion.Network.Serialization;

/// <summary>
/// Extensions to get the byte size for certain types when serializing using an <see cref="INetSerializer"/>.
/// </summary>
public static class SizeExtensions
{
    /// <summary>
    /// Returns the size in bytes of a string encoded using UTF8 serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetSize(this string value)
    {
        return GetSize(value, Encoding.UTF8);
    }

    /// <summary>
    /// Returns the size in bytes of a string serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static int GetSize(this string value, Encoding encoding)
    {
        if (value == null)
        {
            return sizeof(int);
        }

        return encoding.GetByteCount(value) + sizeof(int);
    }

    /// <summary>
    /// Returns the size in bytes of a version serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetSize(this Version value) => sizeof(int) * 3;

    /// <summary>
    /// Returns the size in bytes of a string array encoded using UTF8 serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static int GetSize(this string[] array) => GetSize(array, Encoding.UTF8);

    /// <summary>
    /// Returns the size in bytes of a string array serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static int GetSize(this string[] array, Encoding encoding)
    {
        int size = sizeof(int);

        foreach (var value in array)
        {
            size += value.GetSize(encoding);
        }

        return size;
    }

    /// <summary>
    /// Returns the size in bytes of a string list encoded using UTF8 serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static int GetSize(this List<string> list) => GetSize(list, Encoding.UTF8);

    /// <summary>
    /// Returns the size in bytes of a string list serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static int GetSize(this List<string> list, Encoding encoding)
    {
        int size = sizeof(int);

        foreach (var value in list)
        {
            size += value.GetSize(encoding);
        }

        return size;
    }

    /// <summary>
    /// Returns the size in bytes of a string dictionary encoded using UTF8 serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static int GetSize(this Dictionary<string, string> dictionary) => GetSize(dictionary, Encoding.UTF8);

    /// <summary>
    /// Returns the size in bytes of a string dictionary serialized using an <see cref="INetSerializer"/>.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static int GetSize(this Dictionary<string, string> dictionary, Encoding encoding)
    {
        int size = sizeof(int);

        foreach (var pair in dictionary)
        {
            size += pair.Key.GetSize(encoding) + pair.Value.GetSize(encoding);
        }

        return size;
    }
}
