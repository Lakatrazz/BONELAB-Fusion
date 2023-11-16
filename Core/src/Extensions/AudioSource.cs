using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions
{
    public static class AudioSourceExtensions
    {
        // https://forum.unity.com/threads/audio-realistic-sound-rolloff-tool.543362/
        public static void SetRealisticRolloff(this AudioSource source, float minDistance, float maxDistance)
        {
            var keys = new Keyframe[]
            {
                new Keyframe(minDistance, 1f),
                new Keyframe(minDistance + (maxDistance - minDistance) / 4f, .35f),
                new Keyframe(maxDistance, 0f)
            };

            var animCurve = new AnimationCurve(keys);
            animCurve.SmoothTangents(1, .025f);

            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animCurve);

            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0.1f;
            source.spread = 45f;
        }
    }
}
