using LabFusion.Preferences;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public static class VoiceVolume
{
    public static float GetGlobalVolumeMultiplier()
    {
        float volume = FusionPreferences.ClientSettings.GlobalVolume;

        // If we are loading, lower the volume
        if (FusionSceneManager.IsLoading())
        {
            volume *= 0.25f;
        }

        return volume;
    }
}