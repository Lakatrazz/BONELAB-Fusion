namespace LabFusion.Data;

public static class FileSizeHelper
{
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        long directorySize = 0;

        var directoryInfo = new DirectoryInfo(path);

        foreach (FileInfo file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            directorySize += file.Length;
        }

        return directorySize;
    }
}
