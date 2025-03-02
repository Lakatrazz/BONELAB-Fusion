using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Data;

using LabFusion.Network.Serialization;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedAttack : INetSerializable
{
    public Attack attack;

    public SerializedAttack() { }

    public SerializedAttack(Attack attack)
    {
        this.attack = attack;
    }

    public void Serialize(INetSerializer serializer)
    {
        float damage = 0f;
        AttackType attackType = AttackType.None;
        Vector3 origin = Vector3.zero;
        Vector3 direction = Vector3.zero;
        Vector3 normal = Vector3.zero;

        if (!serializer.IsReader)
        {
            damage = attack.damage;
            attackType = attack.attackType;
            origin = NetworkTransformManager.EncodePosition(attack.origin);
            direction = attack.direction;
            normal = attack.normal;
        }

        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref attackType);
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref normal);

        if (serializer.IsReader)
        {
            attack = new Attack()
            {
                damage = damage,
                attackType = attackType,
                origin = NetworkTransformManager.DecodePosition(origin),
                direction = direction,
                normal = normal,
                backFacing = false,
                collider = null,
                OrderInPool = 0,
                proxy = null,
            };
        }
    }
}
