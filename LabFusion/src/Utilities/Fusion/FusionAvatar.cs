using LabFusion.Marrow;

namespace LabFusion.Utilities;

public static class FusionAvatar
{
    public const string POLY_BLANK_NAME = "char_marrow1_polyBlank";

    public static bool IsMatchingAvatar(string barcode, string target)
    {
        return barcode == target || barcode == BONELABAvatarReferences.PolyBlankBarcode;
    }
}
