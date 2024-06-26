using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct MetadataKVPObject
    {
        [JsonProperty("key")]
        public string Key { get; }

        [JsonProperty("value")]
        public string Value { get; }

        [JsonConstructor]
        public MetadataKVPObject(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
