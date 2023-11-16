using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    public static class FusionPopupManager
    {
        internal static void OnInitializeMelon()
        {
            FusionAchievementPopup.OnInitializeMelon();
        }

        internal static void OnUpdate()
        {
            FusionNotifier.OnUpdate();
            FusionAchievementPopup.OnUpdate();
            FusionBitPopup.OnUpdate();
        }
    }
}
