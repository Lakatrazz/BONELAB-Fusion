using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct DownloadObject
    {
        [JsonProperty("binary_url")]
        public string BinaryUrl { get; }

        [JsonConstructor]
        public DownloadObject(string binaryUrl)
        {
            BinaryUrl = binaryUrl;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
