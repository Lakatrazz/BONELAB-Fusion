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

    public Avatar Avatar { get; set; } = null;

    public AvatarBlendShapeDetector BlendShapeDetector { get; set; } = null;

    public NetworkEntityReference RigReference { get; }

    public RigBlendShapes(NetworkEntityReference rigReference)
    {
        RigReference = rigReference;
    }

    public void OnOwnedUpdate()
    {
        BlendShapeDetector?.CheckForChanges();
    }

    public void OnBlendShapeReceived(AvatarBlendShapePath path)
    {
        if (Avatar == null)
        {
            return;
        }

        var skinnedMeshRenderer = path.GetRenderer(Avatar);

        if (skinnedMeshRenderer == null)
        {
            return;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(path.BlendShapeIndex, ManagedMathf.Clamp(path.BlendShapeValue, BlendShapeMin, BlendShapeMax));
    }

    public void OnAvatarChanged(Avatar avatar)
    {
        Avatar = avatar;

        BlendShapeDetector = new AvatarBlendShapeDetector(avatar);
        BlendShapeDetector.BlendShapeChanged += OnBlendShapeChanged;
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
