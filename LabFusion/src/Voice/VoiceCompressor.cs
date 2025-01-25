using System.IO.Compression;

namespace LabFusion.Voice;

public static class VoiceCompressor
{
    public static byte[] CompressVoiceData(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Fastest);
        deflateStream.Write(data, 0, data.Length);
        deflateStream.Close();
        return compressedStream.ToArray();
    }

    public static byte[] DecompressVoiceData(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}
