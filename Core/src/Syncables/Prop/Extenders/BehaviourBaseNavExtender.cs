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

        protected override void AddToCache(BehaviourBaseNav behaviour, PropSyncable syncable) {
            Cache.Add(behaviour, syncable);
        }

        protected override void RemoveFromCache(BehaviourBaseNav behaviour) {
            Cache.Remove(behaviour);
        }

        public void SwitchLocoState(BehaviourBaseNav.LocoState locoState) {
            BehaviourBaseNavPatches.IgnorePatches = true;
            Component.SwitchLocoState(locoState, 0f, true);
            BehaviourBaseNavPatches.IgnorePatches = false;
        }

        public void SwitchMentalState(BehaviourBaseNav.MentalState mentalState, TriggerRefProxy proxy = null) {
            BehaviourBaseNavPatches.IgnorePatches = true;

            switch (mentalState) {
                default:
                    Component.SwitchMentalState(mentalState);
                    break;
                case BehaviourBaseNav.MentalState.Agroed:
                    Component.SetAgro(proxy);
                    break;
                case BehaviourBaseNav.MentalState.Engaged:
                    Component.SetEngaged(proxy);
                    break;
            }

            BehaviourBaseNavPatches.IgnorePatches = false;
        }
    }
}
