namespace LabFusion.Safety;

public static class ListLoader
{
    internal static void OnInitializeMelon()
    {
        GlobalBanManager.FetchFile();
        ProfanityListManager.FetchFile();
        GlobalModBlacklistManager.FetchFile();
        URLWhitelistManager.FetchFile();
    }
}
