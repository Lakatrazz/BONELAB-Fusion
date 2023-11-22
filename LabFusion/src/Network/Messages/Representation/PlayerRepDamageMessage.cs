using System;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class PlayerRepDamageData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(float);

        public byte damagerId;
        public byte damagedId;
        public float damage;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(damagerId);
            writer.Write(damagedId);
            writer.Write(damage);
        }

        public void Deserialize(FusionReader reader)
        {
            damagerId = reader.ReadByte();
            damagedId = reader.ReadByte();
            damage = reader.ReadSingle();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepDamageData Create(byte damagerId, byte damagedId, float damage)
        {
            return new PlayerRepDamageData
            {
                damagerId = damagerId,
                damagedId = damagedId,
                damage = damage,
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
            using var data = reader.ReadFusionSerializable<PlayerRepDamageData>();
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
                rm.health.TAKEDAMAGE(data.damage);

                // Track the damager
                FusionPlayer.LastAttacker = data.damagerId;
            }
        }
    }
}
