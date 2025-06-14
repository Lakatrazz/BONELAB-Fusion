using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.SDK.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.SDK.Messages;

public class AnimationStateData : INetSerializable
{
    public ComponentPathData ComponentData;

    public int StateNameHash;

    public int Layer;

    public float NormalizedTime;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ComponentData);
        serializer.SerializeValue(ref StateNameHash);
        serializer.SerializeValue(ref Layer);
        serializer.SerializeValue(ref NormalizedTime);
    }
}

public class AnimationStateMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<AnimationStateData>();

        if (data.ComponentData.TryGetComponent<AnimatorSyncer, AnimatorSyncerExtender>(AnimatorSyncer.HashTable, out var animatorSyncer))
        {
            OnFoundAnimatorSyncer(animatorSyncer, data);
        }
    }

    private static void OnFoundAnimatorSyncer(AnimatorSyncer syncer, AnimationStateData data)
    {
        syncer.ApplyAnimationState(data);
    }
}
