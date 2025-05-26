using LabFusion.Player;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public interface INetworkOwnable
{
    PlayerID OwnerID { get; }

    bool IsOwner { get; }

    bool IsOwnerLocked { get; }

    void SetOwner(PlayerID ownerId);

    void LockOwner();

    void UnlockOwner();

    void RemoveOwner();
}
