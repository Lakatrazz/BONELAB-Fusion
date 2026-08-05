using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;
using LabFusion.Marrow.Serialization;
using LabFusion.Entities;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Rig;
using LabFusion.Network;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class RigDamageData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + SerializedAttack.Size + sizeof(byte);

    public NetworkEntityReference RigReference;

    public SerializedAttack Attack;

    public PlayerDamageReceiver.BodyPart Part;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);
        serializer.SerializeValue(ref Attack);
        serializer.SerializeValue(ref Part, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class RigDamageMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigDamageData>();

        var sender = received.SenderSmallID;

        if (!sender.HasValue)
        {
            return;
        }

        var damagerID = sender.Value;

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        if (!networkRig.NetworkEntity.IsOwner)
        {
            return;
        }

        var rm = networkRig.RigRefs.RigManager;
        var health = rm.health;

        var attack = data.Attack.Attack;

        health.OnReceivedDamage(attack, data.Part);

        bool isLocalPlayer = rm.IsLocalPlayer();

        if (isLocalPlayer)
        {
            FusionPlayer.LastAttacker = damagerID;

            LocalHealth.InvokeAttackedByPlayer(attack, data.Part, PlayerIDManager.GetPlayerID(damagerID));
        }
    }
}