using System.Text.Json.Serialization;

namespace LabFusion.Contacts;

/// <summary>
/// A list of contacts given by client platform ID.
/// </summary>
[Serializable]
public sealed class ContactClientList
{
    /// <summary>
    /// The contact info for each client.
    /// </summary>
    [JsonPropertyName("clients")]
    public Dictionary<string, ContactInfo> ContactsByClient { get; } = new();
}
