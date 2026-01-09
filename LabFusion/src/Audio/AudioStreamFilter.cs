using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Math;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.Audio;

[RegisterTypeInIl2Cpp]
public sealed class AudioStreamFilter : MonoBehaviour
{
    public AudioStreamFilter(IntPtr intPtr) : base(intPtr) { }

    public const int QueueCapacity = 8192;

    [HideFromIl2Cpp]
    public Queue<float> ReadingQueue { get; } = new(QueueCapacity);

    [HideFromIl2Cpp]
    public float[] ReadingArray { get; } = new float[AudioInfo.OutputSampleRate];

    [HideFromIl2Cpp]
    public float SampleMultiplier { get; set; } = 1f;

    [HideFromIl2Cpp]
    public float Peak { get; set; } = 1f;

    public const float NormalizationThreshold = 1f;

    [HideFromIl2Cpp]
    public void Enqueue(float sample)
    {
        if (ReadingQueue.Count >= QueueCapacity)
        {
            ReadingQueue.Dequeue();
        }

        ReadingQueue.Enqueue(sample);
    }

    [HideFromIl2Cpp]
    public void TickPeak(float deltaTime)
    {
        Peak = ManagedMathf.Lerp(Peak, 1f, Smoothing.CalculateDecay(4f, deltaTime));
    }

    [HideFromIl2Cpp]
    public void ClearValues()
    {
        ReadingQueue.Clear();
        Peak = 1f;
    }

    private void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
    {
        ProcessAudioFilter(data, channels);
    }

    [HideFromIl2Cpp]
    public void ProcessAudioFilter(Il2CppStructArray<float> data, int channels)
    {
        int length = data.Length;

        int count = length / channels;

        for (var i = 0; i < count; i++)
        {
            float output = 0f;

            if (ReadingQueue.Count > 0)
            {
                output = ReadingQueue.Dequeue();
            }

            ReadingArray[i] = output;

            Peak = MathF.Max(Peak, MathF.Abs(output));
        }

        float voiceNormalizer = 1f;

        if (Peak > NormalizationThreshold)
        {
            voiceNormalizer = NormalizationThreshold / Peak;
        }

        int position = 0;

        var pointer = data.Pointer;
        var pointerSize = IntPtr.Size;

        for (var i = 0; i < length; i += channels)
        {
            float output = ReadingArray[position++];

            for (var j = 0; j < channels; j++)
            {
                var index = i + j;

                var value = InteropUtilities.FloatArrayFastRead(pointer, pointerSize, index);

                // Apply SampleMultiplier after normalization so volume boost actually works
                var result = value * output * AudioInfo.ToneNormalizer * voiceNormalizer * SampleMultiplier;

                InteropUtilities.FloatArrayFastWrite(pointer, pointerSize, index, result);
            }
        }
    }
}
