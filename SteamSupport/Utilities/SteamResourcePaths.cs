namespace MarrowFusion.Steam;

public static class SteamResourcePaths
{
    public static readonly string SteamRelativePath = Path.Combine("Steam");

    public static readonly string LibRelativePath = Path.Combine(SteamRelativePath, "lib");

    public static readonly string x86_64RelativePath = Path.Combine(LibRelativePath, "x86_64");

    public static readonly string SteamAPIWindowsRelativePath = Path.Combine(x86_64RelativePath, "steam_api64.dll");
}
