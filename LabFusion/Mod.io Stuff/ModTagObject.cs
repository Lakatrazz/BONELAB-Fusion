using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModTagObject
    {
        [JsonProperty("tag")]
        public string Tag { get; }

        [JsonConstructor]
        public ModTagObject(string tag)
        {
            Tag = tag;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
