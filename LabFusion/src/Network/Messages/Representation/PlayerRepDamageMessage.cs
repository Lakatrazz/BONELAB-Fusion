using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class PlayerRepDamageData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(float);

    public byte damagerId;
    public byte damagedId;

    public SerializedAttack attack;
    public PlayerDamageReceiver.BodyPart part;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(damagerId);
        writer.Write(damagedId);

        writer.Write(attack);
        writer.Write((ushort)part);
    }

    public void Deserialize(FusionReader reader)
    {
        damagerId = reader.ReadByte();
        damagedId = reader.ReadByte();

        attack = reader.ReadFusionSerializable<SerializedAttack>();
        part = (PlayerDamageReceiver.BodyPart)reader.ReadUInt16();
    }

    public static PlayerRepDamageData Create(byte damagerId, byte damagedId, Attack attack, PlayerDamageReceiver.BodyPart part)
    {
        return new PlayerRepDamageData
        {
            damagerId = damagerId,
            damagedId = damagedId,
            attack = new(attack),
            part = part,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepDamageMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepDamage;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepDamageData>();

        if (data.damagedId != PlayerIdManager.LocalSmallId)
        {
            throw new Exception($"Expected target {data.damagedId}!");
        }

        // Get player health
        var rm = RigData.Refs.RigManager;
        var health = rm.health;

        // Get attack and find the collider
        var attack = data.attack.attack;

        // Track the damager
        FusionPlayer.LastAttacker = data.damagerId;

        health.OnReceivedDamage(attack, data.part);

        LocalHealth.InvokeAttackedByPlayer(attack, data.part, PlayerIdManager.GetPlayerId(data.damagerId));
    }
}