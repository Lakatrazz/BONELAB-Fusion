using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;

using MelonLoader;

using Il2CppCysharp.Threading.Tasks;

namespace LabFusion.Marrow
{
    public static class AudioLoader
    {
        public static void LoadMonoDisc(MonoDiscReference reference, Action<AudioClip> loadCallback)
        {
            var dataCard = reference.DataCard;

            if (dataCard == null)
            {
                return;
            }

            dataCard.AudioClip.LoadAsset(loadCallback);
        }

        public static void LoadMonoDiscs(MonoDiscReference[] references, Action<AudioClip[]> loadCallback)
        {
            MelonCoroutines.Start(LoadMonoDiscsCoroutine(references, loadCallback));
        }

        private static IEnumerator LoadMonoDiscsCoroutine(MonoDiscReference[] references, Action<AudioClip[]> loadCallback)
        {
            AudioClip[] clips = new AudioClip[references.Length];

            for (var i = 0; i < references.Length; i++)
            {
                var reference = references[i];

                var dataCard = reference.DataCard;

                if (dataCard == null)
                {
                    continue;
                }

                var loadTask = dataCard.AudioClip.LoadAssetAsync();

                while (loadTask.Status == UniTaskStatus.Pending)
                {
                    yield return null;
                }

                clips[i] = loadTask.result;
            }

            loadCallback(clips);
        }
    }
}
