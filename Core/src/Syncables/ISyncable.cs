using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Interaction;

namespace LabFusion.Syncables {
    public interface ISyncable {
        void InsertCatchupDelegate(Action<ulong> catchup);

        void InvokeCatchup(ulong user);

        Grip GetGrip(ushort index);

        bool IsGrabbed();

        void SetOwner(byte owner);

        byte? GetOwner();

        void RemoveOwner();

        bool IsOwner();

        bool IsQueued();

        bool IsRegistered();

        void Cleanup();

        void OnRegister(ushort id);

        ushort? GetIndex(Grip grip);

        ushort GetId();

        void OnFixedUpdate();

        void OnUpdate();

        bool IsDestroyed();
    }
}
