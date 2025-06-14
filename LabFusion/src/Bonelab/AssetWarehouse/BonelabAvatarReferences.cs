using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Bonelab;

public static class BonelabAvatarReferences
{
    public static readonly AvatarCrateReference PolyBlankReference = new(PolyBlankBarcode);

    public static readonly AvatarCrateReference StrongReference = new(StrongBarcode);

    public const string PolyBlankBarcode = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

    public const string StrongBarcode = "fa534c5a83ee4ec6bd641fec424c4142.Avatar.Strong";
}