using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Interaction;

namespace LabFusion.Syncables {
    public interface ISyncable {
        Grip GetGrip(ushort index);

        bool IsGrabbed();

        void SetOwner(byte owner);

        byte? GetOwner();

        bool IsQueued();

        void Cleanup();

        void OnRegister(ushort id);

        byte? GetIndex(Grip grip);

        ushort GetId();

        void OnFixedUpdate();
    }
}
