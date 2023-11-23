﻿using SLZ.Interaction;

namespace LabFusion.Syncables
{
    public interface IPropExtender
    {
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
