using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Audio;

using UnityEngine;
using UnityEngine.Audio;

namespace LabFusion.Marrow
{
    public static class SafeAudio3dPlayer
    {
        public static void PlayAtPoint(AudioClip[] clips, Vector3 position, AudioMixerGroup mixer, float volume = 1f, float pitch = 1f)
        {
            Audio3dManager.PlayAtPoint(clips, position, mixer, volume, pitch, new(0f), new(1f), new(1f));
        }
    }
}
