using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

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
    public void Enqueue(float sample)
    {
        if (ReadingQueue.Count >= QueueCapacity)
        {
            ReadingQueue.Dequeue();
        }

        ReadingQueue.Enqueue(sample);
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
                output = ReadingQueue.Dequeue() * SampleMultiplier;
            }

            ReadingArray[i] = output;
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

                InteropUtilities.FloatArrayFastWrite(pointer, pointerSize, index, value * output);
            }
        }
    }
}
