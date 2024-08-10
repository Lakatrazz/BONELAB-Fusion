using System.IO.Compression;

namespace LabFusion.Voice;

public static class VoiceCompressor
{
    public static byte[] CompressVoiceData(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var zipStream = new DeflateStream(compressedStream, CompressionMode.Compress);
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();
        return compressedStream.ToArray();
    }

    public static byte[] DecompressVoiceData(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}
