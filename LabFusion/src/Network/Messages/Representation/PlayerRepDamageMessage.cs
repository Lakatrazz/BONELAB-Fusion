using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;
using LabFusion.Marrow.Serialization;

using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class PlayerRepDamageData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(float);

    public byte damagerId;
    public byte damagedId;

    public SerializableAttack attack;
    public PlayerDamageReceiver.BodyPart part;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref damagerId);
        serializer.SerializeValue(ref damagedId);

        serializer.SerializeValue(ref attack);
        serializer.SerializeValue(ref part, Precision.OneByte);
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

        if (data.damagedId != PlayerIDManager.LocalSmallID)
        {
            throw new Exception($"Expected target {data.damagedId}!");
        }

        // Get player health
        var rm = RigData.Refs.RigManager;
        var health = rm.health;

        // Get attack and find the collider
        var attack = data.attack.Attack;

        // Track the damager
        FusionPlayer.LastAttacker = data.damagerId;

        health.OnReceivedDamage(attack, data.part);

        LocalHealth.InvokeAttackedByPlayer(attack, data.part, PlayerIDManager.GetPlayerID(data.damagerId));
    }
}