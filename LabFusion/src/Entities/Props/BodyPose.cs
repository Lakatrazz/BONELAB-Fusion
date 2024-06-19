using LabFusion.Data;
using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Entities;

public class BodyPose : IFusionSerializable
{
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    public Vector3 velocity = Vector3.zero;
    public Vector3 angularVelocity = Vector3.zero;

    private Vector3 _positionPrediction = Vector3.zero;

    public Vector3 PredictedPosition => position + _positionPrediction;

    public void CopyTo(BodyPose target)
    {
        target.position = position;
        target.rotation = rotation;
        target.velocity = velocity;
        target.angularVelocity = angularVelocity;
    }

    public void ResetPrediction()
    {
        _positionPrediction = Vector3.zero;
    }

    public void PredictPosition(float deltaTime)
    {
        _positionPrediction += velocity * deltaTime;
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(position);
        writer.Write(SerializedSmallQuaternion.Compress(rotation));
        writer.Write(velocity);
        writer.Write(angularVelocity);
    }

    public void Deserialize(FusionReader reader)
    {
        position = reader.ReadVector3();
        rotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>().Expand();
        velocity = reader.ReadVector3();
        angularVelocity = reader.ReadVector3();
    }
}