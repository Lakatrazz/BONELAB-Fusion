using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Data;

using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedAttack : IFusionSerializable
{
    public Attack attack;

    public SerializedAttack() { }

    public SerializedAttack(Attack attack)
    {
        this.attack = attack;
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(attack.damage);
        writer.Write((int)attack.attackType);
        writer.Write(attack.origin);
        writer.Write(attack.direction);
        writer.Write(attack.normal);
    }

    public void Deserialize(FusionReader reader)
    {
        float damage = reader.ReadSingle();
        AttackType attackType = (AttackType)reader.ReadInt32();
        Vector3 origin = reader.ReadVector3();
        Vector3 direction = reader.ReadVector3();
        Vector3 normal = reader.ReadVector3();

        attack = new Attack()
        {
            damage = damage,
            attackType = attackType,
            origin = origin,
            direction = direction,
            normal = normal,
            backFacing = false,
            collider = null,
            OrderInPool = 0,
            proxy = null,
        };
    }
}
