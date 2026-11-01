namespace LabFusion.Permissions;

/// <summary>
/// The level of permissions that a user in the server has assigned.
/// </summary>
public enum PermissionLevel : sbyte
{
    /// <summary>
    /// Someone with less permissions than the normal user.
    /// </summary>
    Guest = -1,

    /// <summary>
    /// The default permission level for a user.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Permissions of a moderator or operator on the server.
    /// </summary>
    Operator = 1,

    /// <summary>
    /// Permissions of an owner on the server.
    /// </summary>
    Owner = 2,
}