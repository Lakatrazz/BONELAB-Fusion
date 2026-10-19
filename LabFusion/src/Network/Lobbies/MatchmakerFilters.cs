namespace LabFusion.Network;

/// <summary>
/// Filters applied to a <see cref="IMatchmaker"/>'s search.
/// </summary>
public struct MatchmakerFilters
{
    public static readonly MatchmakerFilters Default = new()
    {
        FilterFull = true,
        FilterMismatchingVersions = true,
        IncludeFriendPrivateLobbies = false,
    };

    public static readonly MatchmakerFilters Empty = new()
    {
        FilterFull = false,
        FilterMismatchingVersions = false,
        IncludeFriendPrivateLobbies = false,
    };

    public bool FilterFull;

    public bool FilterMismatchingVersions;

    /// <summary>
    /// If true, the search also returns private lobbies hosted by friends that have opted in to friends joining without a code.
    /// </summary>
    public bool IncludeFriendPrivateLobbies;
}
