using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
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

        public PlayerId PlayerId { get; private set; }

        public static Transform[] syncedPoints = new Transform[3];
        public static Transform syncedRoot;

        public Transform[] repTransforms = new Transform[3];
        public Transform repRoot;

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

            // Just a generic material for the spheres since by default they have none
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit (PBR Workflow)"));

            repRoot = new GameObject("Rep Root").transform;
            for (var i = 0; i < repTransforms.Length; i++) {
                repTransforms[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                repTransforms[i].GetComponent<MeshRenderer>().material = mat;
                repTransforms[i].transform.parent = repRoot;
                repTransforms[i].transform.localScale = Vector3.one * (i == 0 ? 0.2f : 0.05f);
            }
        }

        public static void OnRecreateReps(bool isSceneLoad = false) {
            foreach (var rep in Representations.Values) {
                rep.CreateRep(isSceneLoad);
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
                    using (var data = PlayerRepTransformData.Create(PlayerId.SelfId.SmallId, syncedPoints)) {
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
            if (repRoot != null)
                GameObject.Destroy(repRoot.gameObject);

            if (repCanvas != null)
                GameObject.Destroy(repCanvas.gameObject);
        }

        /// <summary>
        /// Checks if all player rep transforms exist
        /// </summary>
        /// <returns></returns>
        public bool HasRepTransforms() {
            return repRoot != null && repCanvas != null;
        }

        public static void OnCachePlayerTransforms() {
            if (RigData.RigManager == null)
                return;

            var heptaRig = RigData.RigManager.remapHeptaRig;

            if (heptaRig)
            {
                syncedRoot = heptaRig.transform;

                syncedPoints[0] = heptaRig.m_head;
                syncedPoints[1] = heptaRig.m_handLf;
                syncedPoints[2] = heptaRig.m_handRt;
            }
        }
    }
}
