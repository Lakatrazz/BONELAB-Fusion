using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Contacts;

public delegate void ContactDelegate(ClientPlatformID platformID, ContactInfo contactInfo);

/// <summary>
/// Manages the saving and loading of player contacts with local settings.
/// </summary>
public static class ContactManager
{
    /// <summary>
    /// The name of the file storing the contacts.
    /// </summary>
    public static readonly string FileName = "contacts.json";

    /// <summary>
    /// The relative path of the contacts to the persistent data folder.
    /// </summary>
    public static string RelativeFilePath => Path.Combine(ResourcePaths.ClientRelativePath, FileName);

    /// <summary>
    /// The actively loaded local contact list.
    /// </summary>
    public static ContactList ContactList { get; private set; } = new();

    /// <summary>
    /// An event invoked whenever a contact changes on the current platform.
    /// </summary>
    public static event ContactDelegate ContactUpdated;

    /// <summary>
    /// Reads the contact list from the file, if available.
    /// </summary>
    public static void ReadFile()
    {
        ContactList = new();

        var deserializedList = JsonSaver.ReadJsonFromFileWithBackup<ContactList>(RelativeFilePath);

        if (deserializedList != null)
        {
            ContactList = deserializedList;
        }
    }

    /// <summary>
    /// Writes the current contact list to the file.
    /// </summary>
    public static void WriteFile()
    {
        JsonSaver.WriteJsonToFileWithBackup(FileName, ContactList);
    }

    /// <summary>
    /// Gets the contact info for a client on a given platform, or returns default info if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static ContactInfo GetContactInfo(string platform, ClientPlatformID platformID)
    {
        var clientList = ContactList.GetClientList(platform);

        if (clientList.ContactsByClient.TryGetValue(platformID.ToString(), out var contactInfo))
        {
            return contactInfo;
        }

        return new ContactInfo();
    }

    /// <summary>
    /// Gets the contact info for a client on the current platform, or returns default info if there is none.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static ContactInfo GetContactInfo(ClientPlatformID platformID) => GetContactInfo(NetworkLayerManager.Platform, platformID);

    /// <summary>
    /// Sets the contact info for a client on a given platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <param name="contactInfo"></param>
    public static void SetContactInfo(string platform, ClientPlatformID platformID, ContactInfo contactInfo)
    {
        var clientList = ContactList.GetClientList(platform);

        clientList.ContactsByClient[platformID.ToString()] = contactInfo;

        WriteFile();

        if (platform == NetworkLayerManager.Platform)
        {
            ContactUpdated?.InvokeSafe(platformID, contactInfo, "invoking ContactManager.ContactUpdated event");
        }
    }

    /// <summary>
    /// Sets the contact info for a client on the current platform.
    /// </summary>
    /// <param name="platformID"></param>
    /// <param name="contactInfo"></param>
    public static void SetContactInfo(ClientPlatformID platformID, ContactInfo contactInfo) => SetContactInfo(NetworkLayerManager.Platform, platformID, contactInfo);
}