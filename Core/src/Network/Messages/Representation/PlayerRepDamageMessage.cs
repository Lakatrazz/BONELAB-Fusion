using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Combat;

namespace LabFusion.Network
{
    public class PlayerRepDamageData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(float);

        public byte damagerId;
        public byte damagedId;

        public float damage;
        public PlayerDamageReceiver.BodyPart part;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(damagerId);
            writer.Write(damagedId);

            writer.Write(damage);
            writer.Write((ushort)part);
        }

        public void Deserialize(FusionReader reader)
        {
            damagerId = reader.ReadByte();
            damagedId = reader.ReadByte();

            damage = reader.ReadSingle();
            part = (PlayerDamageReceiver.BodyPart)reader.ReadUInt16();
        }

        public static PlayerRepDamageData Create(byte damagerId, byte damagedId, float damage, PlayerDamageReceiver.BodyPart part)
        {
            return new PlayerRepDamageData
            {
                damagerId = damagerId,
                damagedId = damagedId,
                damage = damage,
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
            }
            // Otherwise, take the damage
            else if (data.damagedId == PlayerIdManager.LocalSmallId)
            {
                // Get player health
                var rm = RigData.RigReferences.RigManager;

                var attack = new Attack()
                {
                    damage = data.damage,
                    attackType = SLZ.Marrow.Data.AttackType.Piercing,
                };

                // Track the damager
                FusionPlayer.LastAttacker = data.damagerId;

                rm.health.OnReceivedDamage(attack, data.part);
            }
        }
    }
}
