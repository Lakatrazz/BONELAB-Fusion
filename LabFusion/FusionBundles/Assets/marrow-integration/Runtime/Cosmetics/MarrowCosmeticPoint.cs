#if MELONLOADER
using LabFusion.SDK.Points;
#else
using UnityEngine;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    public class MarrowCosmeticPoint : FusionMarrowBehaviour
    {
#else
    public abstract class MarrowCosmeticPoint : FusionMarrowBehaviour {
#endif
#if MELONLOADER
        public MarrowCosmeticPoint(IntPtr intPtr) : base(intPtr) { }

        public virtual AccessoryPoint Point => 0;
#else
        public void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.02f);
        }

        public override string Comment => "Place this script on a GameObject to override the location of a cosmetic! (Must be part of an avatar.)";
#endif
    }
}