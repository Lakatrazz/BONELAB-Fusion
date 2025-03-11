using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Audio;

public static class AudioInfo
{
    public static readonly int OutputSampleRate = AudioSettings.outputSampleRate;

    private static AudioClip _toneClip = null;
    public static AudioClip ToneClip
    {
        get
        {
            if (_toneClip != null)
            {
                return _toneClip;
            }

            _toneClip = AudioClip.Create("Tone", 256, 1, OutputSampleRate, false, (PCMReaderCallback)(data =>
            {
                var length = data.Length;

                for (int i = 0; i < length; i++)
                {
                    data[i] = 1f;
                }
            }));

            return _toneClip;
        }
    }
}
