using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;
using LabFusion.Marrow.Serialization;

using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class PlayerRepDamageData : INetSerializable
{
    public const int Size = SerializedAttack.Size + sizeof(byte);

    public SerializedAttack Attack;

    public PlayerDamageReceiver.BodyPart Part;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Attack);
        serializer.SerializeValue(ref Part, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepDamageMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepDamage;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepDamageData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var damagerID = sender.Value;

        // Get player health
        var rm = RigData.Refs.RigManager;
        var health = rm.health;

        // Get attack and find the collider
        var attack = data.Attack.Attack;

        // Track the damager
        FusionPlayer.LastAttacker = damagerID;

        health.OnReceivedDamage(attack, data.Part);

        LocalHealth.InvokeAttackedByPlayer(attack, data.Part, PlayerIDManager.GetPlayerID(damagerID));
    }
}