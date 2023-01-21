using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using PuppetMasta;
using UnityEngine;

namespace LabFusion.Syncables {
    public class PuppetMasterExtender : PropComponentExtender<PuppetMaster> {
        public static FusionComponentCache<PuppetMaster, PropSyncable> Cache = new FusionComponentCache<PuppetMaster, PropSyncable>();

        protected override void AddToCache(PuppetMaster puppet, PropSyncable syncable) {
            Cache.Add(puppet, syncable);
        }

        protected override void RemoveFromCache(PuppetMaster puppet) {
            Cache.Remove(puppet);
        }

        public override void OnOwnershipTransfer() {
            // Force update slerp drives
            bool isOwner = PropSyncable.IsOwner();

            float muscleWeightMaster = Component.muscleWeight;
            float muscleSpring = Component.muscleSpring;
            float muscleDamper = Component.muscleDamper;

            foreach (var muscle in Component.muscles) {
                if (isOwner) {
                    muscle.SetPdController(muscleWeightMaster, muscleSpring, muscleDamper);
                    muscle.joint.slerpDrive = new JointDrive()
                    {
                        positionSpring = muscle._lastSlerpSpring,
                        positionDamper = muscle._lastSlerpDamper,
                        maximumForce = muscle._lastSlerpMaxF,
                    };
                }
                else {
                    muscle.joint.slerpDrive = default;
                }
            }
        }
    }
}
