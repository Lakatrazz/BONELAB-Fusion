using Il2CppSLZ.Marrow;

using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.Entities;

public class RigSkeleton
{
    public Transform[] TrackedPoints = null;

    public Transform TrackedPlayspace = null;

    public Rigidbody PhysicsPelvis = null;

    public Hand PhysicsLeftHand = null;
    public Hand PhysicsRightHand = null;

    public RemapRig RemapRig = null;

    public Health Health = null;

    public RigSkeleton(RigManager rigManager)
    {
        RigAbstractor.FillTransformArray(ref TrackedPoints, rigManager);

        TrackedPlayspace = rigManager.GetSmoothTurnTransform();

        var physicsRig = rigManager.physicsRig;

        PhysicsPelvis = physicsRig.m_pelvis.GetComponent<Rigidbody>();

        PhysicsLeftHand = physicsRig.leftHand;
        PhysicsRightHand = physicsRig.rightHand;

        RemapRig = rigManager.remapHeptaRig;

        Health = rigManager.health;
    } 
}