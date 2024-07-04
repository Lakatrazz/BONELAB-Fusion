using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

namespace LabFusion.SDK.Gamemodes;

public class HideAndSeek : Gamemode
{
    public override string GamemodeName => "Hide And Seek";

    public override string GamemodeCategory => "Fusion";

    public static class Defaults
    {
        public const int SeekerCount = 2;

        public static readonly MonoDiscReference[] Tracks = new MonoDiscReference[]
        {
            BONELABMonoDiscReferences.TheRecurringDreamReference,
            BONELABMonoDiscReferences.HeavyStepsReference,
            BONELABMonoDiscReferences.StankFaceReference,
            BONELABMonoDiscReferences.AlexInWonderlandReference,
            BONELABMonoDiscReferences.ItDoBeGroovinReference,

            BONELABMonoDiscReferences.ConcreteCryptReference, // concrete crypt
        };
    }

    public int SeekerCount { get; set; } = Defaults.SeekerCount;

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private void SetDefaults()
    {
        Playlist.SetPlaylist(AudioReference.CreateReferences(Defaults.Tracks));
        Playlist.Shuffle();
    }

    protected override void OnStartGamemode()
    {
        base.OnStartGamemode();

        SetDefaults();

        Playlist.StartPlaylist();
    }

    protected override void OnStopGamemode()
    {
        base.OnStopGamemode();

        Playlist.StopPlaylist();
    }

    protected override void OnUpdate()
    {
        if (!IsActive())
        {
            return;
        }

        Playlist.Update();
    }
}
