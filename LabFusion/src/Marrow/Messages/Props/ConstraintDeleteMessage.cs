using LabFusion.Marrow.Patching;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow.Messages;

public class ConstraintDeleteData : INetSerializable
{
    public const int Size = sizeof(ushort);

    public ushort ConstraintID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ConstraintID);
    }
}

[Net.DelayWhileTargetLoading]
public class ConstraintDeleteMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstraintDeleteData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.ConstraintID);

        if (entity == null)
        {
            return;
        }

        var networkConstraint = entity.GetExtender<NetworkConstraint>();

        if (networkConstraint == null)
        {
            return;
        }

        ConstraintTrackerPatches.IgnorePatches = true;

        try
        {
            networkConstraint.Tracker.DeleteConstraint();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("deleting constraint", e);
        }

        ConstraintTrackerPatches.IgnorePatches = false;

#if DEBUG
        FusionLogger.Log($"Unregistered constraint at ID {entity.ID}.");
#endif

        NetworkEntityManager.IDManager.UnregisterEntity(entity);
    }
}