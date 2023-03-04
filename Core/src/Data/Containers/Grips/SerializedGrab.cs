using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;
using SLZ.Marrow.Utilities;

namespace LabFusion.Data {
    public abstract class SerializedGrab : IFusionSerializable {
        public const int Size = sizeof(byte) + SerializedTransform.Size;

        public bool isGrabbed;
        public SerializedTransform targetInBase;
        public GripPair gripPair;

#if DEBUG
        private bool _hasWrittenDefaultGrip = false;
#endif

        public virtual void WriteDefaultGrip(Hand hand, Grip grip) {
            // Check if this is actually grabbed
            isGrabbed = hand.m_CurrentAttachedGO == grip.gameObject;

            // Store the target
            var target = grip.GetTargetInBase(hand);
            targetInBase = new SerializedTransform(target.position, target.rotation);

            gripPair = new GripPair(hand, grip);

#if DEBUG
            _hasWrittenDefaultGrip = true;
#endif
        }

        public virtual void Serialize(FusionWriter writer) {
#if DEBUG
            if (!_hasWrittenDefaultGrip)
                FusionLogger.Warn("Serializing a grab but the default grip values weren't written!");
#endif

            writer.Write(isGrabbed);
            writer.Write(targetInBase);
        }

        public virtual void Deserialize(FusionReader reader) {
            isGrabbed = reader.ReadBoolean();
            targetInBase = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public abstract Grip GetGrip();

        public virtual void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip) {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed || grip == null)
                return;

            rep.AttachObject(handedness, grip, SimpleTransform.Create(targetInBase.position, targetInBase.rotation.Expand()));
        }
    }
}
