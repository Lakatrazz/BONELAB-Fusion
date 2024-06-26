using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ImageObject
    {
        [JsonProperty("filename")]
        public string Filename { get; }

        [JsonProperty("original")]
        public string Original { get; }

        [JsonProperty("thumb_320x180")]
        public string Thumb320x180 { get; }

        [JsonConstructor]
        public ImageObject(string filename, string original, string thumb320x180)
        {
            Filename = filename;
            Original = original;
            Thumb320x180 = thumb320x180;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
