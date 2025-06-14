using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Data;

using LabFusion.Network.Serialization;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Marrow.Serialization;

public class SerializableAttack : INetSerializable
{
    public const int Size = + sizeof(float) * 10 + sizeof(int);

    public Attack Attack;

    public SerializableAttack() { }

    public SerializableAttack(Attack attack)
    {
        this.Attack = attack;
    }

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        float damage = 0f;
        AttackType attackType = AttackType.None;
        Vector3 origin = Vector3.zero;
        Vector3 direction = Vector3.zero;
        Vector3 normal = Vector3.zero;

        if (!serializer.IsReader)
        {
            damage = Attack.damage;
            attackType = Attack.attackType;
            origin = NetworkTransformManager.EncodePosition(Attack.origin);
            direction = Attack.direction;
            normal = Attack.normal;
        }

        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref attackType);
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref normal);

        if (serializer.IsReader)
        {
            Attack = new Attack()
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
