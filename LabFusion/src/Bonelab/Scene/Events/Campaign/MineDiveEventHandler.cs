using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Bonelab.Messages;
using LabFusion.SDK.Scene;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using UnityEngine;
using UnityEngine.Events;

using MelonLoader;

namespace LabFusion.Bonelab.Scene;

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

public class MineDiveEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-54df-470b-baaf-741f4c657665";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-5.6735f, -0.0314f, -11.8826f),
        new(-10.8307f, -0.0054f, 11.0599f),
        new(-11.1408f, 0.3384f, -23.1127f),
        new(-5.2951f, -0.1026f, -34.264f),
        new(-1.4603f, -0.0231f, -24.2549f),
        new(3.4105f, -0.0699f, 10.5926f),
    };

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

    protected override void OnPlayerCatchup(PlayerID playerID)
    {
        if (PrimaryMinecart == null)
        {
            return;
        }

        if (!_hasCreatedCarts)
        {
            return;
        }

        MessageRelay.RelayModule<MineDiveCartMessage, MineDiveCartData>(new MineDiveCartData() { Amount = _cartAmount }, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
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

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        GetCartReferences();

        if (!NetworkInfo.IsHost)
        {
            return;
        }

        _cartAmount = PlayerIDManager.PlayerCount - 1;

        CreateExtraCarts(_cartAmount);

        MessageRelay.RelayModule<MineDiveCartMessage, MineDiveCartData>(new MineDiveCartData() { Amount = _cartAmount }, CommonMessageRoutes.ReliableToOtherClients);
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