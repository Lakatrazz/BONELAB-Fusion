using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Entities;

public class RigSkeleton
{
    public const int TrackerCount = 3;

    public Transform[] TrackedPoints = null;

    public Transform TrackedPlayspace = null;

    public Rigidbody PhysicsPelvis = null;

    public Hand PhysicsLeftHand = null;
    public Hand PhysicsRightHand = null;

    public RemapRig RemapRig = null;

    public Health Health = null;

    public RigSkeleton(RigManager rigManager)
    {
        var openControllerRig = rigManager.ControllerRig.TryCast<OpenControllerRig>();

        GetTrackers(openControllerRig);

        TrackedPlayspace = openControllerRig.vrRoot;

        var physicsRig = rigManager.physicsRig;

        PhysicsPelvis = physicsRig.m_pelvis.GetComponent<Rigidbody>();

        PhysicsLeftHand = physicsRig.leftHand;
        PhysicsRightHand = physicsRig.rightHand;

        RemapRig = rigManager.remapHeptaRig;

        Health = rigManager.health;
    }

    public void GetTrackers(OpenControllerRig openControllerRig)
    {
        TrackedPoints = new Transform[TrackerCount];

        TrackedPoints[0] = openControllerRig.headset;
        TrackedPoints[1] = openControllerRig.leftController.transform;
        TrackedPoints[2] = openControllerRig.rightController.transform;
    }
}