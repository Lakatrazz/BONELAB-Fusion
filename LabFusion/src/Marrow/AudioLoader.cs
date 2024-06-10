using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;

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
    }
}
