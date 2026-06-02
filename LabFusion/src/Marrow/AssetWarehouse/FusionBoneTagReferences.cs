using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

/// <summary>
/// References to BoneTags included in the Fusion Content pallet for SDK functionality.
/// </summary>
public static class FusionBoneTagReferences
{
    /// <summary>
    /// The BoneTag that replaces the Player tag on Net Players.
    /// </summary>
    public static readonly BoneTagReference FusionPlayerReference = new("Lakatrazz.FusionContent.BoneTag.FusionPlayer");

    /// <summary>
    /// The BoneTag used to identify the LavaGang team for gamemodes.
    /// Not currently attached to rigs.
    /// </summary>
    public static readonly BoneTagReference TeamLavaGangReference = new("Lakatrazz.FusionContent.BoneTag.TeamLavaGang");

    /// <summary>
    /// The BoneTag used to identify the Sabrelake team for gamemodes.
    /// Not currently attached to rigs.
    /// </summary>
    public static readonly BoneTagReference TeamSabrelakeReference = new("Lakatrazz.FusionContent.BoneTag.TeamSabrelake");

    /// <summary>
    /// The BoneTag used to identify invisible spectators in gamemodes.
    /// Not currently attached to rigs.
    /// </summary>
    public static readonly BoneTagReference SpectatorReference = new("Lakatrazz.FusionContent.BoneTag.Spectator");
}