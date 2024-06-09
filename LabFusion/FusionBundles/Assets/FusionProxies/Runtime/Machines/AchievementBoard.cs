using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class AchievementBoard : MonoBehaviour
    {
#if MELONLOADER
        public AchievementBoard(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            // Setup the achievement board logic
            CupBoardHelper.CompleteAchievementBoard(gameObject);
        }
#endif
    }
}