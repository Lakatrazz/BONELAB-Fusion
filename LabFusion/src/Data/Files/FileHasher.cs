using System.Security.Cryptography;

namespace LabFusion.Data;

/// <summary>
/// Functions for computing hashes or comparing files by hash.
/// </summary>
public static class FileHasher
{
    /// <summary>
    /// Returns true if the hashes of a file and a byte array match.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileBytes"></param>
    /// <returns></returns>
    public static bool FileMatchesBytes(string filePath, byte[] fileBytes)
    {
        using var sha256 = SHA256.Create();

        using var stream = File.OpenRead(filePath);

        byte[] diskHash = sha256.ComputeHash(stream);
        byte[] embeddedHash = sha256.ComputeHash(fileBytes);

        return diskHash.SequenceEqual(embeddedHash);
    }

    /// <summary>
    /// Returns true if the hashes of two files match.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileBytes"></param>
    /// <returns></returns>
    public static bool FileMatchesFile(string firstFilePath, string secondFilePath)
    {
        using var sha256 = SHA256.Create();

        using var firstStream = File.OpenRead(firstFilePath);
        using var secondStream = File.OpenRead(secondFilePath);

        byte[] firstHash = sha256.ComputeHash(firstStream);
        byte[] secondHash = sha256.ComputeHash(secondStream);

        return firstHash.SequenceEqual(secondHash);
    }
}