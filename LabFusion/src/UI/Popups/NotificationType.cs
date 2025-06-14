namespace LabFusion.UI.Popups;

/// <summary>
/// The basic types of notifications that can be sent.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Used to inform the user.
    /// </summary>
    INFORMATION = 0,

    /// <summary>
    /// Used when the user should be notified of a potential issue.
    /// </summary>
    WARNING = 1,

    /// <summary>
    /// Used when the user or program attempts a task and it fails.
    /// </summary>
    ERROR = 2,

    /// <summary>
    /// Used when the user or program performs a task and it suceeds.
    /// </summary>
    SUCCESS = 3,
}
