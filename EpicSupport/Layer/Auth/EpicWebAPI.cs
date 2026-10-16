using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarrowFusion.Epic;

internal class EpicWebAPI
{
    private const string Scope = "basic_profile";
    private const string DeviceAuthUrl = "https://api.epicgames.dev/epic/oauth/v2/deviceAuthorization";
    private const string TokenUrl = "https://api.epicgames.dev/epic/oauth/v2/token";

    private readonly HttpClient http;

    internal EpicWebAPI()
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        
        http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    internal async Task<DeviceAuthResponse> BeginDeviceAuthAsync()
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = EOSPlatform.ClientId
        });

        using var resp = await http.PostAsync(DeviceAuthUrl, content).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
            throw new EpicWebAPIException("Device authorization failed", resp.StatusCode, body);

        return JsonSerializer.Deserialize<DeviceAuthResponse>(body);
    }
    
    private async Task<(EpicTokenResponse Token, HttpStatusCode Status, string Body)> RequestTokenAsync(Dictionary<string, string> grantParams)
    {
        var form = new Dictionary<string, string>(grantParams)
        {
            ["deployment_id"] = EOSPlatform.DeploymentId,
            ["scope"] = Scope
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
        req.Content = new FormUrlEncodedContent(form);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{EOSPlatform.ClientId}:{EOSPlatform.ClientSecret}")));

        using var resp = await http.SendAsync(req).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        var token = resp.IsSuccessStatusCode ? JsonSerializer.Deserialize<EpicTokenResponse>(body) : null;
        return (token, resp.StatusCode, body);
    }
    
    internal async Task<EpicTokenResponse> WaitForDeviceAuthAsync(DeviceAuthResponse device)
    {
        if (device.Interval <= 0 || device.ExpiresIn <= 0)
            throw new ArgumentException("Invalid device authorization polling interval", nameof(device));

        var deadline = DateTimeOffset.UtcNow.AddSeconds(device.ExpiresIn);
        var interval = device.Interval;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var (token, status, body) = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "device_code",
                ["device_code"] = device.DeviceCode
            }).ConfigureAwait(false);

            if (token != null)
                return token;

            if (Contains(body, "slow_down"))
                interval += 5;
            else if (Contains(body, "expired_token") || Contains(body, "access_denied"))
                throw new EpicWebAPIException("Device authorization failed", status, body);
            
            await Task.Delay(TimeSpan.FromSeconds(interval)).ConfigureAwait(false);
        }

        throw new TimeoutException("Device authorization timed out");
    }
    
    internal async Task<EpicTokenResponse> RefreshAsync(string refreshToken)
    {
        var (token, status, body) = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        }).ConfigureAwait(false);

        return token ?? throw new EpicWebAPIException("Token refresh failed", status, body);
    }

    private static bool Contains(string body, string value) => body?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
}

internal class DeviceAuthResponse
{
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; } = "";

    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; } = "";

    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; } = "";

    [JsonPropertyName("verification_uri_complete")]
    public string VerificationUriComplete { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}

internal class EpicTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonPropertyName("refresh_expires_at")]
    public DateTimeOffset RefreshExpiresAt { get; set; }

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = "";

    [JsonPropertyName("selected_account_id")]
    public string SelectedAccountId { get; set; }
}

internal class EpicWebAPIException : Exception
{
    public EpicWebAPIException(string message, HttpStatusCode status, string body) : base(message)
    {
        Status = status;
        ResponseBody = body;
    }

    public HttpStatusCode Status { get; }
    public string ResponseBody { get; }
}