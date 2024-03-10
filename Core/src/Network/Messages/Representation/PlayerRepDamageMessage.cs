using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Combat;
using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class PlayerRepDamageData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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

                var attack = new Attack()
                {
                    damage = data.damage,
                    attackType = SLZ.Marrow.Data.AttackType.Piercing,
                };

                rm.health.OnReceivedDamage(attack, data.part);

                // Track the damager
                FusionPlayer.LastAttacker = data.damagerId;
            }
        }
    }
}
