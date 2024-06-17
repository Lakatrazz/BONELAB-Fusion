using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public interface INetworkOwnable
{
    PlayerId OwnerId { get; }

    bool IsOwner { get; }

    bool IsOwnerLocked { get; }

    void SetOwner(PlayerId ownerId);

    void LockOwner();

    void UnlockOwner();

    void RemoveOwner();
}
