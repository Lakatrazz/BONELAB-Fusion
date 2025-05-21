namespace LabFusion.SDK.Metadata;

public static class GamemodeKeys
{
    public const string StartedKey = "Gamemode.Started";

    public const string SelectedKey = "Gamemode.Selected";

    public const string ReadyKey = "Gamemode.Ready";

    public static Predicate<string> PersistentKeys => (v) =>
    {
        switch (v)
        {
            case StartedKey:
            case SelectedKey:
            case ReadyKey:
                return true;
            default:
                return false;
        }
    };
}