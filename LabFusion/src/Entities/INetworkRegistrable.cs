using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public interface INetworkRegistrable
{
    ushort Id { get; }

    ushort QueueId { get; }

    bool IsRegistered { get; }

    bool IsQueued { get; }

    bool IsDestroyed { get; }

    void Register(ushort id);

    void Queue(ushort queuedId);

    void Unregister();
}