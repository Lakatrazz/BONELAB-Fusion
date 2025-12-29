using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Audio;

public static class AudioInfo
{
    /// <summary>
    /// The output sample rate for audio sources.
    /// </summary>
    public static readonly int OutputSampleRate = AudioSettings.outputSampleRate;

    /// <summary>
    /// The value of each sample in <see cref="CreateToneClip"/>.
    /// This is set to a small value for stability.
    /// </summary>
    public static readonly float ToneVolume = 0.1f;

    /// <summary>
    /// The value that, when multiplied, will restore samples from <see cref="CreateToneClip"/> to 1.
    /// </summary>
    public static readonly float ToneNormalizer = 1f / ToneVolume;

    /// <summary>
    /// Creates an AudioClip with all samples set to <see cref="ToneVolume"/>.
    /// </summary>
    /// <returns></returns>
    public static AudioClip CreateToneClip()
    {
        return AudioClip.Create("Tone", OutputSampleRate, 1, OutputSampleRate, false, (PCMReaderCallback)(data =>
        {
            var length = data.Length;

            for (int i = 0; i < length; i++)
            {
                data[i] = ToneVolume;
            }
        }));
    }
}
