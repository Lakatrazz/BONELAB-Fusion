namespace MarrowFusion.Epic;

public static class EpicResourcePaths
{
    public static readonly string EpicRelativePath = Path.Combine("Epic");

    public static readonly string DexRelativePath = Path.Combine(EpicRelativePath, "dex");

    public static readonly string LibRelativePath = Path.Combine(EpicRelativePath, "lib");

    public static readonly string x86_64RelativePath = Path.Combine(LibRelativePath, "x86_64");

    public static readonly string Arm64RelativePath = Path.Combine(LibRelativePath, "arm64");

    public static readonly string EOSSDKWindowsRelativePath = Path.Combine(x86_64RelativePath, "EOSSDK-Win64-Shipping.dll");

    public static readonly string EOSSDKAndroidRelativePath = Path.Combine(Arm64RelativePath, "libEOSSDK.so");
}
