using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct FilehashObject
    {
        [JsonProperty("md5")]
        public string Md5 { get; }

        [JsonConstructor]
        public FilehashObject(string md5)
        {
            Md5 = md5;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
