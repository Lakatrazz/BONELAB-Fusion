using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;
using LabFusion.Marrow.Integration;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WristWatchReferences
{
    public static AudioPlayerSettings WatchAudioPlayerSettings => new()
    {
        Mixer = LocalAudioPlayer.HardInteraction,
        Volume = 0.2f,
    };

    public Transform Root { get; set; } = null;
    public UIElementView RootView { get; set; } = null;

    public Animator Animator { get; set; } = null;

    public Transform Origin { get; set; } = null;
    public Transform Surface { get; set; } = null;
    public Transform Panel { get; set; } = null;

    public RectTransform Canvas { get; set; } = null;

    public Transform Head { get; set; } = null;

    public RigManager RigManager { get; set; } = null;

    public Transform ControllerRigTransform { get; set; } = null;

    public Transform Effects { get; set; } = null;

    public Transform UI { get; set; } = null;

    public List<LineRenderer> Lines { get; } = new();

    public List<Transform> Corners { get; } = new();

    public List<Transform> Flares { get; } = new();

    public void PlaySound(MonoDiscReference monoDiscReference)
    {
        LocalAudioPlayer.PlayAtPoint(new AudioReference(monoDiscReference), Origin.position, WatchAudioPlayerSettings);
    }

    public void GetReferences(Transform root, RigManager rigManager)
    {
        Root = root;
        RigManager = rigManager;

        Animator = root.GetComponent<Animator>();

        UI = root.Find("UI");

        Origin = UI.Find("Origin");
        Panel = Origin.Find("Panel");
        Surface = Origin.Find("Surface");

        Canvas = Panel.Find("Canvas").GetComponent<RectTransform>();

        RootView = Canvas.Find("view_UIElement").GetComponent<UIElementView>();

        Head = rigManager.ControllerRig.TryCast<OpenControllerRig>().headset;
        ControllerRigTransform = rigManager.ControllerRig.transform;

        Lines.Clear();
        Corners.Clear();
        Flares.Clear();

        Effects = root.Find("Effects");

        var effectsOrigin = Effects.Find("Origin");

        var linesRoot = effectsOrigin.Find("Lines");
        var cornersRoot = Panel.Find("Corners");
        var flaresRoot = effectsOrigin.Find("Flares");

        for (var i = 0; i < linesRoot.childCount; i++)
        {
            Lines.Add(linesRoot.GetChild(i).GetComponent<LineRenderer>());
        }

        for (var i = 0; i < cornersRoot.childCount; i++)
        {
            Corners.Add(cornersRoot.GetChild(i));
        }

        for (var i = 0; i < flaresRoot.childCount; i++)
        {
            Flares.Add(flaresRoot.GetChild(i));
        }
    }
}
