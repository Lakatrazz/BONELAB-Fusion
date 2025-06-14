using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Audio;

public static class AudioInfo
{
    public static readonly int OutputSampleRate = AudioSettings.outputSampleRate;

    public static AudioClip CreateToneClip()
    {
        return AudioClip.Create("Tone", OutputSampleRate, 1, OutputSampleRate, false, (PCMReaderCallback)(data =>
        {
            var length = data.Length;

            for (int i = 0; i < length; i++)
            {
                data[i] = 1f;
            }
        }));
    }
}
