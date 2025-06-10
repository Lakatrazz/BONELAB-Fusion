using GroovyCodecs.G711.aLaw;
using LabFusion.Utilities;
using System.IO.Compression;

namespace LabFusion.Voice;

public static class VoiceConverter
{
    public static readonly ALawEncoder G711Encoder = new();
    public static readonly ALawDecoder G711Decoder = new();

    /// <summary>
    /// Copies an array of 32 bit samples to an array of 16 bit samples.
    /// </summary>
    /// <param name="from">The 32 bit array to copy from. The length should be equal to or greater than sampleCount.</param>
    /// <param name="to">The 16 bit array to copy to. The length should be equal to or greater than sampleCount.</param>
    /// <param name="sampleCount">The amount of samples.</param>
    public static void CopySamples(float[] from, short[] to, int sampleCount)
    {
        for (var i = 0; i < sampleCount; i++)
        {
            to[i] = ConvertSample(from[i]);
        }
    }

    /// <summary>
    /// Copies an array of 16 bit samples to an array of 32 bit samples.
    /// </summary>
    /// <param name="from">The 16 bit array to copy to. The length should be equal to or greater than sampleCount.</param>
    /// <param name="to">The 32 bit array to copy from. The length should be equal to or greater than sampleCount.</param>
    /// <param name="sampleCount">The amount of samples.</param>
    public static void CopySamples(short[] from, float[] to, int sampleCount)
    {
        for (var i = 0; i < sampleCount; i++)
        {
            to[i] = ConvertSample(from[i]);
        }
    }

    /// <summary>
    /// Converts a 32 bit sample into a 16 bit sample.
    /// </summary>
    /// <param name="sample"></param>
    /// <returns></returns>
    public static short ConvertSample(float sample)
    {
        return (short)(sample * short.MaxValue);
    }

    /// <summary>
    /// Converts a 16 bit sample into a 32 bit sample.
    /// </summary>
    /// <param name="sample"></param>
    /// <returns></returns>
    public static float ConvertSample(short sample)
    {
        return ((float)sample) / short.MaxValue;
    }

    /// <summary>
    /// Encodes and compresses a 16 bit array of samples into a byte array.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    public static byte[] Encode(short[] samples)
    {
        return Compress(G711Encoder.Process(samples));
    }

    /// <summary>
    /// Decompresses and decodes an array of bytes into a 16 bit array of samples.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static short[] Decode(byte[] bytes)
    {
        return G711Decoder.Process(Decompress(bytes));
    }

    public static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var deflateStream = new DeflateStream(compressedStream, CompressionLevel.SmallestSize);
        deflateStream.Write(data, 0, data.Length);
        deflateStream.Close();
        return compressedStream.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}
