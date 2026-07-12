namespace LabFusion.Data;

public static class FileSizeHelper
{
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        long fileSize = 0;

        try
        {
            foreach (string filePath in Directory.EnumerateFiles(path))
            {
                fileSize += new FileInfo(filePath).Length;
            }
        }
        catch (UnauthorizedAccessException) { }

        try
        {
            foreach (string directoryPath in Directory.EnumerateDirectories(path))
            {
                fileSize += GetDirectorySize(directoryPath);
            }
        }
        catch (UnauthorizedAccessException) { }

        return fileSize;
    }
}
