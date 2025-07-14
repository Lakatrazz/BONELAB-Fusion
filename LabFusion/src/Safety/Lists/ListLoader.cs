namespace LabFusion.Safety;

public static class ListLoader
{
    internal static void OnInitializeMelon()
    {
#if DEBUG
        return;
#endif
        GlobalBanManager.FetchFile();
        ProfanityListManager.FetchFile();
        GlobalModBlacklistManager.FetchFile();
        URLWhitelistManager.FetchFile();
    }
}
