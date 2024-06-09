using LabFusion.Patching;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.PuppetMasta;
using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Syncables
{
    public class BehaviourBaseNavExtender : PropComponentExtender<BehaviourBaseNav>
    {
        public static FusionComponentCache<BehaviourBaseNav, PropSyncable> Cache = new FusionComponentCache<BehaviourBaseNav, PropSyncable>();

        protected override void AddToCache(BehaviourBaseNav behaviour, PropSyncable syncable)
        {
            Cache.Add(behaviour, syncable);
        }

        protected override void RemoveFromCache(BehaviourBaseNav behaviour)
        {
            Cache.Remove(behaviour);
        }

        public void SwitchLocoState(BehaviourBaseNav.LocoState locoState)
        {
            BehaviourBaseNavPatches.IgnorePatches = true;
            Component.SwitchLocoState(locoState, 0f, true);
            BehaviourBaseNavPatches.IgnorePatches = false;
        }

        public void SwitchMentalState(BehaviourBaseNav.MentalState mentalState, TriggerRefProxy proxy = null)
        {
            BehaviourBaseNavPatches.IgnorePatches = true;

            switch (mentalState)
            {
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
