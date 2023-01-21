using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using PuppetMasta;

using SLZ.AI;

using UnityEngine;

namespace LabFusion.Syncables {
    public class BehaviourBaseNavExtender : PropComponentExtender<BehaviourBaseNav> {
        public static FusionComponentCache<BehaviourBaseNav, PropSyncable> Cache = new FusionComponentCache<BehaviourBaseNav, PropSyncable>();

        public bool IsValidForOverrides = false;

        private BehaviourBaseNav.LocoState _prevLocoState;

        protected override void AddToCache(BehaviourBaseNav behaviour, PropSyncable syncable) {
            Cache.Add(behaviour, syncable);

            if (behaviour.TryCast<BehaviourPowerLegs>() || behaviour.TryCast<BehaviourOmniwheel>())
                IsValidForOverrides = true;
        }

        protected override void RemoveFromCache(BehaviourBaseNav behaviour) {
            Cache.Remove(behaviour);
        }

        public override void OnOwnedUpdate() {
            // Make sure this is valid
            if (!IsValidForOverrides)
                return;

            // Send loco state change
            var state = Component.locoState;

            if (state != _prevLocoState)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = BehaviourBaseNavLocoData.Create(PlayerIdManager.LocalSmallId, PropSyncable, Component))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.BehaviourBaseNavLoco, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }

                _prevLocoState = state;
            }
        }

        public void SetLocoState(BehaviourBaseNav.LocoState locoState) {
            BehaviourBaseNavPatches.IgnorePatches = true;
            Component.SwitchLocoState(locoState, 0f, true);
            _prevLocoState = locoState;
            BehaviourBaseNavPatches.IgnorePatches = false;
        }
    }
}
