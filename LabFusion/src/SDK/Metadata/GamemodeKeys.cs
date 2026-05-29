namespace LabFusion.SDK.Metadata;

public static class GamemodeKeys
{
    public const string StartedKey = "Gamemode.Started";

    public const string SelectedKey = "Gamemode.Selected";

    public const string ReadyKey = "Gamemode.Ready";

    public const string ElapsedKey = "Gamemode.Elapsed";

    public static Predicate<string> PersistentKeys => (v) =>
    {
        return v switch
        {
            StartedKey or
            SelectedKey or 
            ReadyKey => true,
            _ => false,
        };
    };
}