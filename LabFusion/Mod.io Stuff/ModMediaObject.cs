using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModMediaObject
    {
        [JsonProperty("youtube")]
        public IReadOnlyList<string> Youtube { get; }

        [JsonProperty("sketchfab")]
        public IReadOnlyList<string> Sketchfab { get; }

        [JsonProperty("images")]
        public IReadOnlyList<ImageObject> Images { get; }

        [JsonConstructor]
        public ModMediaObject(IReadOnlyList<string> youtube, IReadOnlyList<string> sketchfab, IReadOnlyList<ImageObject> images)
        {
            Youtube = youtube;
            Sketchfab = sketchfab;
            Images = images;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
