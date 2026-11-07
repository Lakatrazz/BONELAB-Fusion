using System.Text.Json.Serialization;

namespace LabFusion.Contacts;

/// <summary>
/// A serializable list of contacts used to store settings for players.
/// </summary>
[Serializable]
public sealed class ContactList
{
    /// <summary>
    /// The list of contact info for clients by the network layer platform.
    /// </summary>
    [JsonPropertyName("contacts")]
    public Dictionary<string, ContactClientList> ContactsByPlatform { get; } = new();

    /// <summary>
    /// Get a list of contacts by client for the current platform, or returns a new list if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public ContactClientList GetClientList(string platform)
    {
        if (ContactsByPlatform.TryGetValue(platform, out var contacts))
        {
            return contacts;
        }

        contacts = new ContactClientList();
        ContactsByPlatform[platform] = contacts;

        return contacts;
    }
}
