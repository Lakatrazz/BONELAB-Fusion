using Il2CppSLZ.VRMK;

namespace LabFusion.Marrow.Extensions;

public static class AvatarExtensions
{
    public static void LoadSurfaceData(this Avatar avatar)
    {
        if (avatar.surfaceData != null)
        {
            return;
        }

        var surfaceDataCardReference = avatar.surfaceDataCard;

        if (surfaceDataCardReference == null || !surfaceDataCardReference.IsValid())
        {
            return;
        }

        var dataCard = surfaceDataCardReference.DataCard;

        if (dataCard == null)
        {
            return;
        }

        var surfaceData = dataCard.SurfaceData.Asset;

        if (surfaceData == null)
        {
            return;
        }

        avatar.surfaceData = surfaceData;
    }
}
