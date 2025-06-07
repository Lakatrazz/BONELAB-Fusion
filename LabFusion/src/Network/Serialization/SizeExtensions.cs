using System.Text;

namespace LabFusion.Network.Serialization;

public static class SizeExtensions
{
    public static int GetSize(this string value)
    {
        return GetSize(value, Encoding.UTF8);
    }

    public static int GetSize(this string value, Encoding encoding)
    {
        return encoding.GetByteCount(value) + sizeof(int);
    }

    public static int GetSize(this Version value) => sizeof(int) * 3;

    public static int GetSize(this string[] array) => GetSize(array, Encoding.UTF8);

    public static int GetSize(this string[] array, Encoding encoding)
    {
        int size = sizeof(int);

        foreach (var value in array)
        {
            size += value.GetSize(encoding);
        }

        return size;
    }

    public static int GetSize(this List<string> list) => GetSize(list, Encoding.UTF8);

    public static int GetSize(this List<string> list, Encoding encoding)
    {
        int size = sizeof(int);

        foreach (var value in list)
        {
            size += value.GetSize(encoding);
        }

        return size;
    }

    public static int GetSize(this Dictionary<string, string> dictionary)
    {
        int size = sizeof(int);

        foreach (var pair in dictionary)
        {
            size += pair.Key.GetSize() + pair.Value.GetSize();
        }

        return size;
    }
}
