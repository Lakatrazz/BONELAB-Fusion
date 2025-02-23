using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace LabFusion.SDK.Messages;

public class AnimationStateData : IFusionSerializable
{
    public ComponentPathData ComponentData { get; set; }

    public int StateNameHash { get; set; }

    public int Layer { get; set; }

    public float NormalizedTime { get; set; }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ComponentData);

        writer.Write(StateNameHash);

        writer.Write(Layer);

        writer.Write(NormalizedTime);
    }

    public void Deserialize(FusionReader reader)
    {
        ComponentData = reader.ReadFusionSerializable<ComponentPathData>();

        StateNameHash = reader.ReadInt32();

        Layer = reader.ReadInt32();

        NormalizedTime = reader.ReadSingle();
    }
}

public class AnimationStateMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<AnimationStateData>();

        if (data.ComponentData.hasNetworkEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.ComponentData.entityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<AnimatorSyncerExtender>();

            if (extender == null)
            {
                return;
            }

            var animatorSyncer = extender.GetComponent(data.ComponentData.componentIndex);

            if (animatorSyncer == null)
            {
                return;
            }

            OnFoundAnimatorSyncer(animatorSyncer, data);
        }
        else
        {
            var animatorSyncer = AnimatorSyncer.HashTable.GetComponentFromData(data.ComponentData.hashData);

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
