using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Player;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Bonelab;

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
public class PlayerRepDamageMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PlayerRepDamage;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepDamageData>();

        // If we are the server, relay it to the desired user
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.SendFromServer(data.damagedId, NetworkChannel.Reliable, message);
            return;
        }

        // Otherwise, take the damage
        if (data.damagedId == PlayerIdManager.LocalSmallId)
        {
            // Get player health
            var rm = RigData.RigReferences.RigManager;
            var health = rm.health;

            // Get attack and find the collider
            var attack = data.attack.attack;

            // Track the damager
            FusionPlayer.LastAttacker = data.damagerId;

            health.OnReceivedDamage(attack, data.part);
        }
    }
}