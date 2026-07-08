using LabFusion.Preferences.Client;

namespace LabFusion.Safety;

public static class URLWhitelistManager
{
    /// <summary>
    /// Domains that are allowed, but only if the client has "Allow Untrusted URLs" enabled.
    /// Typically domains that point to direct, unmoderated. files.
    /// </summary>
    public static readonly List<string> UntrustedDomains = new()
    {
        "cdn.discordapp.com",
        "imgur.com",
        "video.twimg.com",
        "catbox.moe",
        "files.catbox.moe",
        "litter.catbox.moe",
        "drive.google.com",
        "packaged-media.redd.it",
        "www.dropbox.com",
        "archive.org",
        "dl.dropboxusercontent.com",
        "drive.usercontent.google.com",
        "d.uguu.se",
    };

    /// <summary>
    /// Domains that are always allowed for video players.
    /// Typically domains for videos hosted on moderated websites.
    /// </summary>
    public static readonly List<string> TrustedDomains = new();

    public static bool IsURLWhitelisted(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return true;
        }

        bool isLink = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;

        if (!isLink)
        {
            return true;
        }

        var domain = uri.Host;

        if (TrustedDomains.Contains(domain))
        {
            return true;
        }

        bool allowUntrustedURLs = ClientSettings.Safety.AllowUntrustedURLs.Value;

        if (allowUntrustedURLs && UntrustedDomains.Contains(domain))
        {
            return true;
        }

        return false;
    }
}
