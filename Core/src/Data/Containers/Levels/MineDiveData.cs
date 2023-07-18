﻿using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Vehicle;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace LabFusion.Data
{
    public class MineDiveData : LevelDataHandler
    {
        public static GameObject Minecart;
        public static GameObject AvatarCart;
        public static GameObject InvisibleMinecart;

        private static readonly float LocalOffset = -1.581f;
        private static readonly Vector3 JointAnchor = new(0f, 0.283f, 0.795f);
        const int MaxExtraCarts = 7;

        private static int _cartAmount = 0;
        private static bool _hasCreatedCarts = false;

        protected override void PlayerCatchup(ulong longId) {
            if (Minecart != null && _hasCreatedCarts) {
                using var writer = FusionWriter.Create();
                using var data = MineDiveCartData.Create(_cartAmount);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.MineDiveCart, writer);
                MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
            }
        }

        protected override void SceneAwake() {
            _hasCreatedCarts = false;

            Minecart = GameObject.Find("Minecart Gun Variant");
            AvatarCart = GameObject.Find("Avatar-Gun Variant (1)");

            if (Minecart != null) {
                Transform tempParent = new GameObject().transform;
                tempParent.gameObject.SetActive(false);

                InvisibleMinecart = GameObject.Instantiate(Minecart, tempParent);
                InvisibleMinecart.SetActive(false);
                InvisibleMinecart.name = "Fusion Invisible Minecart";

                InvisibleMinecart.transform.parent = Minecart.transform.parent;
                GameObject.Destroy(tempParent.gameObject);
            }
        }

        protected override void MainSceneInitialized() {
            if (NetworkInfo.IsServer) {
                _cartAmount = PlayerIdManager.PlayerCount - 1;
                CreateExtraCarts(_cartAmount);

                using var writer = FusionWriter.Create();
                using var data = MineDiveCartData.Create(_cartAmount);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.MineDiveCart, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

        public static void CreateExtraCarts(int amount)
        {
            if (_hasCreatedCarts || amount <= 0)
                return;

            if (Minecart != null)
            {
                var minecartColliders = Minecart.GetComponentsInChildren<Collider>(true);
                var avatarcartColliders = AvatarCart.GetComponentsInChildren<Collider>(true);

                var splineTrack = GameObject.Find("MineCart-Track-01-Player-Track").GetComponent<SplineJoint>();

                Transform lastCart = Minecart.transform;

                List<Collider> colliders = new List<Collider>();
                colliders.AddRange(minecartColliders);
                colliders.AddRange(avatarcartColliders);

                List<Transform> carts = new List<Transform>();

                for (var i = 0; i < amount && i < MaxExtraCarts; i++)
                {
                    // Create minecarts
                    var newCart = GameObject.Instantiate(InvisibleMinecart);
                    carts.Add(newCart.transform);
                    newCart.transform.parent = Minecart.transform.parent;
                    newCart.name = $"{Minecart.name} Player {i + 1}";

                    // Setup spline
                    var body = newCart.GetComponent<SplineBody>();
                    body.SetMaximumForce(0f);
                    body.SetPositionDamper(0f);
                    body.SetTargetVelocity(0f);

                    var otherJoints = body.GetComponents<ConfigurableJoint>();
                    foreach (var splineJoint in otherJoints)
                    {
                        GameObject.DestroyImmediate(splineJoint);
                    }

                    body.AttachToSplineJoint(splineTrack);

                    // Disable extras
                    newCart.transform.Find("gun_minecart").gameObject.SetActive(false);
                    newCart.transform.Find("mineCart_Light").gameObject.SetActive(false);
                    newCart.transform.Find("mineCart_birdcage").gameObject.SetActive(false);

                    GameObject.DestroyImmediate(newCart.GetComponent<MineCartControl>());

                    // Configure lap bar
                    var lapBar = newCart.GetComponentInChildren<MineCartLapBar>();
                    lapBar.OnSeat = new UnityEvent();
                    lapBar.OnUnseat = new UnityEvent();
                    lapBar.OnBarLocked = new UnityEvent();

                    // Configure seat
                    var seat = newCart.GetComponentInChildren<Seat>();
                    seat.GetComponent<BoxCollider>().enabled = true;

                    // Add joint
                    newCart.transform.position = lastCart.transform.position + (lastCart.transform.forward * LocalOffset);
                    newCart.transform.rotation = lastCart.transform.rotation;

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

                    joint.connectedMassScale = 0f;

                    joint.autoConfigureConnectedAnchor = false;
                    Vector3 anchor = joint.transform.TransformPoint(JointAnchor);

                    joint.anchor = joint.transform.InverseTransformPoint(anchor);
                    joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(anchor);

                    colliders.AddRange(newCart.GetComponentsInChildren<Collider>(true));

                    newCart.gameObject.SetActive(true);

                    lastCart = newCart.transform;
                }

                // Ignore colliders between each other
                foreach (var first in colliders) {
                    foreach (var second in colliders) {
                        Physics.IgnoreCollision(first, second, true);
                    }
                }

                // Parent all carts
                foreach (var cart in carts) {
                    cart.transform.parent = Minecart.transform;
                }

                // Re-enable collision between front cart and avatar cart
                foreach (var first in minecartColliders) {
                    foreach (var second in avatarcartColliders) {
                        Physics.IgnoreCollision(first, second, false);
                    }
                }
            }

            _hasCreatedCarts = true;
        }
    }
}
