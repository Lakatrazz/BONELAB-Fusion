using Il2CppSLZ.Interaction;
using Il2CppSLZ.Rig;

using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.Entities;

public class RigSkeleton
{
    public Transform[] trackedPoints = null;

    public Transform trackedPlayspace = null;

    public Rigidbody physicsPelvis = null;

    public Hand physicsLeftHand = null;
    public Hand physicsRightHand = null;

    public RemapRig remapRig = null;

    public RigSkeleton(RigManager rigManager)
    {
        RigAbstractor.FillTransformArray(ref trackedPoints, rigManager);

        trackedPlayspace = rigManager.GetSmoothTurnTransform();

        var physicsRig = rigManager.physicsRig;

        physicsPelvis = physicsRig.m_pelvis.GetComponent<Rigidbody>();

        physicsLeftHand = physicsRig.leftHand;
        physicsRightHand = physicsRig.rightHand;

        remapRig = rigManager.remapHeptaRig;
    } 
}