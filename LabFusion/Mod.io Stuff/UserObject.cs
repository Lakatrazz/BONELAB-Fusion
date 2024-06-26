using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct UserObject
    {
        [JsonProperty("id")]
        public long Id { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonConstructor]
        public UserObject(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
