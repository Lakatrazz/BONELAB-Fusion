using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable {
        public static readonly Dictionary<byte, PlayerRep> Representations = new Dictionary<byte, PlayerRep>();
        public static readonly Dictionary<OpenControllerRig, PlayerRep> PlayerRepControllers = new Dictionary<OpenControllerRig, PlayerRep>();

        public PlayerId PlayerId { get; private set; }

        public static Transform[] syncedPoints = new Transform[3];
        public static Transform syncedControllerRig;
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;

        public SerializedTransform[] serializedTransforms = new SerializedTransform[3];

        public Transform[] repTransforms = new Transform[3];
        public OpenControllerRig repControllerRig;
        public Rigidbody repPelvis;
        public RigManager rigManager;

        public GameObject repCanvas;
        public Canvas repCanvasComponent;
        public Transform repCanvasTransform;
        public TextMeshProUGUI repNameText;

        public PlayerRep(PlayerId playerId)
        {
            PlayerId = playerId;
            Representations.Add(playerId.SmallId, this);

            CreateRep();
        }

        public void CreateRep(bool isSceneLoad = false) {
            // Not necessary to create a new one
            if (isSceneLoad && HasRepTransforms()) {
                return;
            }

            // Make sure we don't have any extra objects
            DestroyRep();

            repCanvas = new GameObject("RepCanvas");
            repCanvasComponent = repCanvas.AddComponent<Canvas>();

            repCanvasComponent.renderMode = RenderMode.WorldSpace;
            repCanvasTransform = repCanvas.transform;
            repCanvasTransform.localScale = Vector3.one / 200.0f;

            repNameText = repCanvas.AddComponent<TextMeshProUGUI>();

            repNameText.alignment = TextAlignmentOptions.Midline;
            repNameText.enableAutoSizing = true;

            repNameText.text = "Placeholder";

            rigManager = PlayerRepUtilities.CreateNewRig();
            rigManager.physicsRig.m_pelvis.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

            PlayerRepControllers.Add(rigManager.openControllerRig, this);

            repPelvis = rigManager.physicsRig.m_pelvis.GetComponent<Rigidbody>();
            repControllerRig = rigManager.openControllerRig;
            
            repTransforms[0] = rigManager.openControllerRig.m_head;
            repTransforms[1] = rigManager.openControllerRig.m_handLf;
            repTransforms[2] = rigManager.openControllerRig.m_handRt;
        }

        public static void OnRecreateReps(bool isSceneLoad = false) {
            foreach (var rep in Representations.Values) {
                rep.CreateRep(isSceneLoad);
            }
        }

        public void OnUpdateTransforms() {
            for (var i = 0; i < 3; i++) {
                repTransforms[i].localPosition = serializedTransforms[i].position;
                repTransforms[i].localRotation = serializedTransforms[i].rotation.Expand();
            }
        }

        public static void OnVerifyReps() {
            foreach (var rep in Representations.Values) {
                if (!rep.HasRepTransforms())
                    rep.CreateRep();
            }
        }

        private static bool TrySendRep() {
            try {
                foreach (var syncPoint in syncedPoints)
                    if (syncPoint == null)
                        return false;

                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerRepTransformData.Create(PlayerId.SelfId.SmallId, syncedPoints, syncedPelvis, syncedControllerRig, syncedPlayspace)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepTransform, writer)) {
                            FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Unreliable, message);
                        }
                    }
                }

                return true;
            } 
            catch (Exception e) {
#if DEBUG
                FusionLogger.Error($"Failed sending player transforms with reason: {e.Message}\nTrace:{e.StackTrace}");
#endif
            }
            return false;
        }

        public static void OnSyncRep() {
            if (NetworkUtilities.IsInServer) {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
            }
        }

        public static void OnUpdateTrackers() {

        }

        /// <summary>
        /// Destroys anything about the PlayerRep and frees it from memory.
        /// </summary>
        public void Dispose() {
            Representations.Remove(PlayerId.SmallId);

            DestroyRep();

            GC.SuppressFinalize(this);

#if DEBUG
            FusionLogger.Log($"Disposed PlayerRep with small id {PlayerId.SmallId}");
#endif
        }

        /// <summary>
        /// Destroys the GameObjects of the PlayerRep. Does not free it from memory or remove it from its slots. Use Dispose for that.
        /// </summary>
        public void DestroyRep() {
            if (rigManager != null)
                GameObject.Destroy(rigManager.gameObject);

            if (repCanvas != null)
                GameObject.Destroy(repCanvas.gameObject);
        }

        /// <summary>
        /// Checks if all player rep transforms exist
        /// </summary>
        /// <returns></returns>
        public bool HasRepTransforms() {
            return rigManager != null && repCanvas != null;
        }

        public static void OnCachePlayerTransforms() {
            if (RigData.RigManager == null)
                return;

            var heptaRig = RigData.RigManager.openControllerRig;

            if (heptaRig)
            {
                syncedPelvis = RigData.RigManager.physicsRig.m_pelvis;
                syncedControllerRig = RigData.RigManager.openControllerRig.transform;
                syncedPlayspace = RigData.RigManager.openControllerRig.vrRoot;

                syncedPoints[0] = heptaRig.m_head;
                syncedPoints[1] = heptaRig.m_handLf;
                syncedPoints[2] = heptaRig.m_handRt;
            }
        }
    }
}
