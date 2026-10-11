using LabFusion.Marrow.Data;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Rig;
using LabFusion.Math;
using LabFusion.Network;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Entities;

public class RigBlendShapes
{
    public const float BlendShapeMin = 0f, BlendShapeMax = 100f;

    public Avatar Avatar { get; private set; } = null;

    public AvatarBlendShapeDetector BlendShapeDetector { get; private set; } = null;

    public NetworkEntityReference RigReference { get; private set; } = default;

    public bool IsInitialized { get; private set; } = false;

    public bool IsReady => IsInitialized && Avatar != null;

    public void Initialize(NetworkEntityReference rigReference)
    {
        RigReference = rigReference;
        IsInitialized = true;
    }

    public void AssignAvatar(Avatar avatar)
    {
        Avatar = avatar;

        BlendShapeDetector = new AvatarBlendShapeDetector(avatar);
        BlendShapeDetector.BlendShapeChanged += OnBlendShapeChanged;
    }

    public void OnOwnedUpdate()
    {
        if (!IsReady)
        {
            return;
        }

        BlendShapeDetector?.CheckForChanges();
    }

    public void OnBlendShapeReceived(AvatarBlendShapePath path)
    {
        if (!IsReady)
        {
            return;
        }

        if (!path.TryGetRenderer(Avatar, out var renderer))
        {
            return;
        }

        renderer.SetBlendShapeWeight(path.BlendShapeIndex, ManagedMathf.Clamp(path.BlendShapeValue, BlendShapeMin, BlendShapeMax));
    }

    private void OnBlendShapeChanged(AvatarBlendShapePath path)
    {
        var data = new RigBlendShapeData()
        {
            RigReference = RigReference,
            Path = path,
        };

        ClientManager.RelayModule<RigBlendShapeMessage, RigBlendShapeData>(data, CommonMessageRoutes.UnreliableToOtherClients);
    }
}
