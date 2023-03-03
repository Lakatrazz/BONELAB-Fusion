using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Syncables {
    public interface IPropExtender {
        PropSyncable PropSyncable { get; set; }

        bool ValidateExtender(PropSyncable syncable);
        
        void OnCleanup();

        void OnOwnedUpdate();

        void OnReceivedUpdate();

        void OnOwnershipTransfer();

        void OnUpdate();

        void OnAttach(Hand hand, Grip grip);

        void OnDetach(Hand hand, Grip grip);

        void OnHeld();
    }
}
