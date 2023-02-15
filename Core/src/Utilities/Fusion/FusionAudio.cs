using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class FusionAudio {
        public static AudioSource Play2D(AudioClip clip, float volume = 1f) {
            GameObject go = new GameObject("Fusion 2D Audio Source");
            var source = go.AddComponent<AudioSource>();
            source.volume = volume;
            source.clip = clip;
            source.spatialBlend = 0f;

            source.Play();

            return source;
        }

        public static AudioSource Play3D(Vector3 position, AudioClip clip, float volume = 1f, bool loop = false) {
            GameObject go = new GameObject("Fusion 3D Audio Source");
            var source = go.AddComponent<AudioSource>();
            go.transform.position = position;

            source.volume = volume;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.loop = loop;

            source.Play();

            return source;
        }
    }
}
