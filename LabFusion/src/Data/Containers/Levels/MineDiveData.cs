﻿using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using UnityEngine;
using UnityEngine.Events;

using MelonLoader;

namespace LabFusion.Data;

[RegisterTypeInIl2Cpp]
public class MineCartFollower : MonoBehaviour
{
    public MineCartFollower(IntPtr intPtr) : base(intPtr) { }

    public MineCartControl TargetControl { get; set; }
    public MineCartControl SelfControl { get; set; }

    private void LateUpdate()
    {
        SelfControl.UpdateSpeed((int)TargetControl.rideSpeed);
    }
}

public class MineDiveData : LevelDataHandler
{
    public override string LevelTitle => "04 - Mine Dive";

    public const string PrimaryMinecartName = "Minecart Gun Variant";
    public const string SecondaryMinecartName = "Minecart NPC Variant";

    public static readonly string[] MinecartExtras = new string[]
    {
        "gun",
        "gun_minecart",
        "mineCart_Light",
        "mineCart_birdcage",
    };

    public static GameObject PrimaryMinecart { get; private set; }
    public static GameObject SecondaryMinecart { get; private set; }
    public static GameObject TemplateMinecart { get; private set; }

    public static GameObject EntranceDoor { get; private set; }

    private static readonly float LocalOffset = -1.581f;
    private static readonly Vector3 JointAnchor = new(0f, 0.283f, 0.795f);

    private const int MaxExtraCarts = 7;

    private static int _cartAmount = 0;
    private static bool _hasCreatedCarts = false;

    protected override void PlayerCatchup(PlayerId playerId)
    {
        if (PrimaryMinecart == null)
        {
            return;
        }

        if (!_hasCreatedCarts)
        {
            return;
        }

        using var writer = FusionWriter.Create();
        var data = MineDiveCartData.Create(_cartAmount);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.MineDiveCart, writer);
        MessageSender.SendFromServer(playerId, NetworkChannel.Reliable, message);
    }

    private static void GetCartReferences()
    {
        _hasCreatedCarts = false;

        EntranceDoor = GameObject.Find("MineDiveDoors");

        if (EntranceDoor == null)
        {
            FusionLogger.Warn($"Could not find the Entrance Doors in MineDive, please report this to {FusionMod.ModAuthor}!");
        }

        PrimaryMinecart = GameObject.Find(PrimaryMinecartName);

        if (PrimaryMinecart != null)
        {
            TemplateMinecart = CreateTemplateFromMinecart(PrimaryMinecart);
        }
        else
        {
            FusionLogger.Warn($"Could not find the Primary Minecart GameObject in MineDive, please report this to {FusionMod.ModAuthor}!");
        }

        SecondaryMinecart = GameObject.Find(SecondaryMinecartName);

        if (SecondaryMinecart == null)
        {
            FusionLogger.Warn($"Could not find the Secondary Minecart GameObject in MineDive, please report this to {FusionMod.ModAuthor}!");
        }
    }

    private static GameObject CreateTemplateFromMinecart(GameObject minecart)
    {
        // Create a disabled version of the minecart
        Transform templateParent = new GameObject().transform;
        templateParent.gameObject.SetActive(false);

        var templateMinecart = GameObject.Instantiate(minecart, templateParent);
        templateMinecart.SetActive(false);
        templateMinecart.name = "Fusion Template Minecart";

        var templateTransform = templateMinecart.transform;

        templateTransform.parent = minecart.transform.parent;
        GameObject.Destroy(templateParent.gameObject);

        // Unfreeze the rigidbodies
        var rigidbodies = templateMinecart.GetComponentsInChildren<Rigidbody>();

        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }

        // Disable extras
        foreach (var extra in MinecartExtras)
        {
            templateTransform.Find(extra).gameObject.SetActive(false);
        }

        // Configure lap bar
        var lapBar = templateMinecart.GetComponentInChildren<MineCartLapBar>();
        lapBar.OnSeat = new UnityEvent();
        lapBar.OnUnseat = new UnityEvent();
        lapBar.OnBarLocked = new UnityEvent();

        // Configure seat
        var seat = templateMinecart.GetComponentInChildren<Seat>();
        seat.GetComponent<BoxCollider>().enabled = true;

        return templateMinecart;
    }

    protected override void MainSceneInitialized()
    {
        GetCartReferences();

        if (!NetworkInfo.IsServer)
        {
            return;
        }

        _cartAmount = PlayerIdManager.PlayerCount - 1;

        CreateExtraCarts(_cartAmount);

        using var writer = FusionWriter.Create();
        var data = MineDiveCartData.Create(_cartAmount);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.MineDiveCart, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    public static void CreateExtraCarts(int amount)
    {
        if (_hasCreatedCarts || amount <= 0)
        {
            return;
        }

        if (PrimaryMinecart == null)
        {
            return;
        }

        var primaryMinecartColliders = PrimaryMinecart.GetComponentsInChildren<Collider>(true);
        var secondaryMinecartColliders = SecondaryMinecart.GetComponentsInChildren<Collider>(true);

        var entranceDoorColliders = EntranceDoor.GetComponentsInChildren<Collider>(true);

        var splineTrack = GameObject.Find("MineCart-Track-01-Player-Track").GetComponent<SplineJoint>();

        Transform lastCart = PrimaryMinecart.transform;

        var colliders = new List<Collider>();
        colliders.AddRange(primaryMinecartColliders);
        colliders.AddRange(secondaryMinecartColliders);
        colliders.AddRange(entranceDoorColliders);

        var carts = new List<Transform>();

        int totalCarts = 1;

        for (var i = 0; i < amount && i < MaxExtraCarts; i++)
        {
            // Add a new minecart
            var newCart = CreateMinecart(i, splineTrack, lastCart);

            lastCart = newCart.transform;

            colliders.AddRange(newCart.GetComponentsInChildren<Collider>(true));

            totalCarts++;
        }

        // Ignore colliders between each other
        foreach (var first in colliders)
        {
            foreach (var second in colliders)
            {
                Physics.IgnoreCollision(first, second, true);
            }
        }

        // Parent all carts
        foreach (var cart in carts)
        {
            cart.transform.parent = PrimaryMinecart.transform;
        }

        // Re-enable collision between front cart and avatar cart
        foreach (var first in primaryMinecartColliders)
        {
            foreach (var second in secondaryMinecartColliders)
            {
                Physics.IgnoreCollision(first, second, false);
            }
        }

        _hasCreatedCarts = true;
    }

    private static GameObject CreateMinecart(int index, SplineJoint splineTrack, Transform lastCart)
    {
        // Instantiate a new cart from the template cart
        var newCart = GameObject.Instantiate(TemplateMinecart);

        newCart.transform.parent = PrimaryMinecart.transform.parent;
        newCart.name = $"{PrimaryMinecartName} Player {index + 1}";

        // Setup the spline
        var splineBody = newCart.GetComponent<SplineBody>();

        var otherJoints = splineBody.GetComponents<ConfigurableJoint>();
        foreach (var splineJoint in otherJoints)
        {
            GameObject.DestroyImmediate(splineJoint);
        }

        splineBody.AttachToSplineJoint(splineTrack);

        // Add joint to the last minecart
        newCart.transform.position = lastCart.position + (lastCart.forward * LocalOffset);
        newCart.transform.rotation = lastCart.rotation;

        var joint = newCart.gameObject.AddComponent<ConfigurableJoint>();
        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit() { limit = 0.3f };

        joint.xDrive = joint.yDrive = joint.zDrive = new JointDrive()
        {
            positionSpring = 500000f,
            positionDamper = 10000f,
            maximumForce = 500000f,
        };

        joint.connectedBody = lastCart.gameObject.GetComponent<Rigidbody>();

        joint.autoConfigureConnectedAnchor = false;
        Vector3 anchor = joint.transform.TransformPoint(JointAnchor);

        joint.anchor = joint.transform.InverseTransformPoint(anchor);
        joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(anchor);

        // Don't apply any forces to the primary minecart
        // In the future this may be removed, but is currently very difficult with the current minecart setup
        joint.connectedMassScale = 0f;

        // Activate the minecart
        newCart.gameObject.SetActive(true);

        // Add a follower so it can increase it's speed to match the main minecart
        var follower = newCart.AddComponent<MineCartFollower>();
        follower.SelfControl = newCart.GetComponent<MineCartControl>();
        follower.TargetControl = PrimaryMinecart.GetComponent<MineCartControl>();

        return newCart;
    }
}