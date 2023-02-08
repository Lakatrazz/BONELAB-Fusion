using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public interface INetworkLobby {
        void SetMetadata(string key, string value);

        bool TryGetMetadata(string key, out string value);

        string GetMetadata(string key);

        Action CreateJoinDelegate(LobbyMetadataInfo info);
    }
}
