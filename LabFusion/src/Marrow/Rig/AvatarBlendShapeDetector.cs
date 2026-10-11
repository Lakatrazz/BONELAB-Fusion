using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Marrow.Data;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Rig;

public class AvatarBlendShapeDetector
{
    public static readonly float ChangeThreshold = 1f;

    public Dictionary<int, float[]> BodyMeshBlendShapes { get; } = new();
    public Dictionary<int, float[]> HeadMeshBlendShapes { get; } = new();
    public Dictionary<int, float[]> HairMeshBlendShapes { get; } = new();

    public Avatar Avatar { get; private set; } = null;

    public event Action<AvatarBlendShapePath> BlendShapeChanged;

    public AvatarBlendShapeDetector(Avatar avatar)
    {
        AssignAvatar(avatar);
    }

    public void CheckForChanges()
    {
        CheckForChanges(AvatarMeshType.Body, Avatar.bodyMeshes, BodyMeshBlendShapes);
        CheckForChanges(AvatarMeshType.Head, Avatar.headMeshes, HeadMeshBlendShapes);
        CheckForChanges(AvatarMeshType.Hair, Avatar.hairMeshes, HairMeshBlendShapes);
    }

    private void CheckForChanges(AvatarMeshType meshType, Il2CppReferenceArray<SkinnedMeshRenderer> renderers, Dictionary<int, float[]> blendShapes)
    {
        if (blendShapes.Count <= 0)
        {
            return;
        }

        foreach (var blendShapePair in blendShapes)
        {
            var index = blendShapePair.Key;
            var shapes = blendShapePair.Value;

            var renderer = renderers[index];

            for (var i = 0; i < shapes.Length; i++)
            {
                float currentShape = shapes[i];
                float newShape = renderer.GetBlendShapeWeight(i);

                if (MathF.Abs(newShape - currentShape) >= ChangeThreshold)
                {
                    shapes[i] = newShape;

                    var blendShapePath = new AvatarBlendShapePath()
                    {
                        MeshType = meshType,
                        MeshIndex = index,
                        BlendShapeIndex = i,
                        BlendShapeValue = newShape,
                    };

                    BlendShapeChanged?.Invoke(blendShapePath);
                }
            }
        }
    }

    private void AssignAvatar(Avatar avatar)
    {
        Avatar = avatar;

        AssignBlendShapes(avatar.bodyMeshes, BodyMeshBlendShapes);
        AssignBlendShapes(avatar.headMeshes, HeadMeshBlendShapes);
        AssignBlendShapes(avatar.hairMeshes, HairMeshBlendShapes);
    }

    private static void AssignBlendShapes(Il2CppReferenceArray<SkinnedMeshRenderer> renderers, Dictionary<int, float[]> blendShapes)
    {
        for (var i = 0; i < renderers.Count; i++)
        {
            var renderer = renderers[i];

            if (renderer == null)
            {
                continue;
            }

            var sharedMesh = renderer.sharedMesh;

            if (sharedMesh == null)
            {
                continue;
            }

            var blendShapeCount = sharedMesh.blendShapeCount;

            if (blendShapeCount <= 0)
            {
                continue;
            }

            blendShapes[i] = new float[blendShapeCount];
        }
    }
}
