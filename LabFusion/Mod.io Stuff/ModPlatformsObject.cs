using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModPlatformsObject
    {
        [JsonProperty("platform")]
        public string Platform { get; }

        [JsonProperty("status")]
        public int Status { get; }

        [JsonConstructor]
        public ModPlatformsObject(string platform, int status)
        {
            Platform = platform;
            Status = status;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
