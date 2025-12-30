using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

namespace LabFusion.Bonelab.SDK;

public static class BonelabMusicManager
{
    public static void Initialize()
    {
        FusionMonoDiscPlaylists.AmbientPlaylist = new MonoDiscReference[]
        {
            BonelabMonoDiscReferences.TheRecurringDreamReference,
            BonelabMonoDiscReferences.HeavyStepsReference,
            BonelabMonoDiscReferences.StankFaceReference,
            BonelabMonoDiscReferences.AlexInWonderlandReference,
            BonelabMonoDiscReferences.ItDoBeGroovinReference,

            BonelabMonoDiscReferences.ConcreteCryptReference, // concrete crypt
        };
    }
}
