using LabFusion.Data;
using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public class EntityPose : IFusionSerializable   
{
    public BodyPose[] bodies;

    public EntityPose() { }

    public EntityPose(int bodyCount)
    {
        bodies = new BodyPose[bodyCount];

        for (var i = 0; i < bodyCount; i++)
        {
            bodies[i] = new BodyPose();
        }
    }

    public void CopyTo(EntityPose target)
    {
        if (target.bodies.Length != bodies.Length) 
        {
            return;
        }

        for (var i = 0; i < target.bodies.Length; i++)
        {
            bodies[i].CopyTo(target.bodies[i]);
        }
    }

    public void Serialize(FusionWriter writer)
    {
        byte length = (byte)bodies.Length;

        writer.Write(length);

        for (var i = 0; i < length; i++)
        {
            writer.Write(bodies[i]);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        byte length = reader.ReadByte();

        bodies = new BodyPose[length];

        for (var i = 0; i < length; i++)
        {
            bodies[i] = reader.ReadFusionSerializable<BodyPose>();
        }
    }
}