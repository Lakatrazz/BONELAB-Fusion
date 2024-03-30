using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Lobbies
{
    public interface ILobbyFilter
    {
        string GetTitle();

        bool IsActive();

        void SetActive(bool active);

        bool FilterLobby(INetworkLobby lobby, LobbyMetadataInfo info);
    }
}
