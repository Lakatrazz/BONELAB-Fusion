namespace LabFusion.Data;

/// <summary>
/// Tools for copying files between paths.
/// </summary>
public static class FileCopier
{
    /// <summary>
    /// Copies all files in a directory from one path to another recursively.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public static void CopyDirectoryRecursive(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destination, fileName);
            File.Copy(file, destFile, true);
        }

        foreach (string subDir in Directory.GetDirectories(source))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destination, dirName);
            CopyDirectoryRecursive(subDir, destSubDir);
        }
    }
}
