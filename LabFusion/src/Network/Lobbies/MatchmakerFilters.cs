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
    };

    public static readonly MatchmakerFilters Empty = new()
    {
        FilterFull = false,
        FilterMismatchingVersions = false,
    };

    public bool FilterFull;

    public bool FilterMismatchingVersions;
}
