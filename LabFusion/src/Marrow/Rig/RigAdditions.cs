using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Rig;

public static class RigAdditions
{
    public static event Action<RigManager> OnApplyRigAdditions;
    public static event Action<RigManager> OnRemoveRigAdditions;

    public static event Action<RigManager> OnApplyLocalRigAdditions;
    public static event Action<RigManager> OnRemoveLocalRigAdditions;

    public static event Action<RigManager> OnApplyNetRigAdditions;
    public static event Action<RigManager> OnRemoveNetRigAdditions;

    internal static void Initialize()
    {
        MultiplayerHooking.OnJoinedServer += () => { ApplyLocalRigAdditions(RigData.Refs.RigManager); };
        MultiplayerHooking.OnStartedServer += () => { ApplyLocalRigAdditions(RigData.Refs.RigManager); };
        MultiplayerHooking.OnDisconnected += () => { RemoveLocalRigAdditions(RigData.Refs.RigManager); };
        LocalPlayer.OnLocalRigCreated += OnLocalRigCreated;
    }

    private static void OnLocalRigCreated(RigManager rigManager)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        ApplyLocalRigAdditions(rigManager);
    }

    public static void ApplyRigAdditions(RigManager rigManager)
    {
        OnApplyRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveRigAdditions(RigManager rigManager)
    {
        OnRemoveRigAdditions?.Invoke(rigManager);
    }

    public static void ApplyLocalRigAdditions(RigManager rigManager)
    {
        ApplyRigAdditions(rigManager);

        OnApplyLocalRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveLocalRigAdditions(RigManager rigManager)
    {
        RemoveRigAdditions(rigManager);

        OnRemoveLocalRigAdditions?.Invoke(rigManager);
    }

    public static void ApplyNetRigAdditions(RigManager rigManager)
    {
        ApplyRigAdditions(rigManager);

        OnApplyNetRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveNetRigAdditions(RigManager rigManager)
    {
        RemoveRigAdditions(rigManager);

        OnRemoveNetRigAdditions?.Invoke(rigManager);
    }
}
