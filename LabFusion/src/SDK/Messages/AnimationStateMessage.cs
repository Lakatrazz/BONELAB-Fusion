using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
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

        if (data.ComponentData.HasEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.ComponentData.EntityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<AnimatorSyncerExtender>();

            if (extender == null)
            {
                return;
            }

            var animatorSyncer = extender.GetComponent(data.ComponentData.ComponentIndex);

            if (animatorSyncer == null)
            {
                return;
            }

            OnFoundAnimatorSyncer(animatorSyncer, data);
        }
        else
        {
            var animatorSyncer = AnimatorSyncer.HashTable.GetComponentFromData(data.ComponentData.HashData);

            if (animatorSyncer == null)
            {
                return;
            }

            OnFoundAnimatorSyncer(animatorSyncer, data);
        }
    }

    private static void OnFoundAnimatorSyncer(AnimatorSyncer syncer, AnimationStateData data)
    {
        syncer.ApplyAnimationState(data);
    }
}
