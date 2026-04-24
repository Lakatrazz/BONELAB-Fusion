using LabFusion.Network;

namespace LabFusion.Safety;

public enum TrustedStatus
{
    /// <summary>
    /// No trust status. This is a regular user.
    /// </summary>
    None,

    /// <summary>
    /// Username matches that of a trusted user, but ID doesn't.
    /// </summary>
    Impersonator,

    /// <summary>
    /// Username and ID match that of a well-known community member. NOT IMPLEMENTED YET
    /// </summary>
    Trusted,

    /// <summary>
    /// Username and ID match that of a global moderator.
    /// </summary>
    Master,
}

public static class TrustedListManager
{
    public struct TrustedPlayer
    {
        public string ID;
        public string Name;
        public bool Unique;

        public TrustedPlayer(string id, string name, bool unique = true)
        {
            this.ID = id;
            this.Name = name;
            this.Unique = unique;
        }
    }

    private static readonly TrustedPlayer[] _steamPlayers = new TrustedPlayer[] {
        // Fusion testers
        new("76561198198752494", "Lakatrazz"),
        new("76561198097630377", "AlexTheBaBa"),
        new("76561198222917852", "Mr.Gaming"),
        new("76561198096586464", "brwok"),
        new("76561198143565238", "Riggle"),
        new("76561198233973112", "Alfie", false),
        new("76561198061847729", "zz0000"),
        new("76561198837064193", "172", false),
        new("76561198147092613", "Eli", false),
    };

    public static TrustedStatus VerifyPlayer(string id, string name)
    {
        if (NetworkLayerManager.Layer is SteamNetworkLayer)
        {
            return VerifyPlayer(_steamPlayers, id, name);
        }

        return TrustedStatus.None;
    }

    private static TrustedStatus VerifyPlayer(TrustedPlayer[] players, string id, string name)
    {
        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];

            // Our id matches, and this is a master user
            if (player.ID == id)
            {
                return TrustedStatus.Master;
            }

            // Convert names to have no whitespace and in lowercase
            string masterName = TextFilter.SanitizeName(player.Name).ToLower();

            string otherName = TextFilter.SanitizeName(name).ToLower();

            // The name matches, but the id didn't
            if (otherName.Contains(masterName) && player.Unique)
            {
                return TrustedStatus.Impersonator;
            }
        }

        // Neither name nor id matched, this is a regular joe
        return TrustedStatus.None;
    }
}