using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModObject
    {
        [JsonProperty("id")]
        public long Id { get; }

        [JsonProperty("game_id")]
        public long GameId { get; }

        [JsonProperty("status")]
        public int Status { get; }

        [JsonProperty("visible")]
        public int Visible { get; }

        [JsonProperty("submitted_by")]
        public UserObject SubmittedBy { get; }

        [JsonProperty("date_added")]
        public long DateAdded { get; }

        [JsonProperty("date_updated")]
        public long DateUpdated { get; }

        [JsonProperty("date_live")]
        public long DateLive { get; }

        [JsonProperty("maturity_option")]
        public int MaturityOption { get; }

        [JsonProperty("community_options")]
        public int CommunityOptions { get; }

        [JsonProperty("monetisation_options")]
        public int MonetisationOptions { get; }

        [JsonProperty("price")]
        public int Price { get; }

        [JsonProperty("tax")]
        public int Tax { get; }

        [JsonProperty("logo")]
        public LogoObject Logo { get; }

        [JsonProperty("homepage_url")]
        public string HomepageUrl { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("name_id")]
        public string NameId { get; }

        [JsonProperty("summary")]
        public string Summary { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("description_plaintext")]
        public string DescriptionPlaintext { get; }

        [JsonProperty("metadata_blob")]
        public string MetadataBlob { get; }

        [JsonProperty("profile_url")]
        public string ProfileUrl { get; }

        [JsonProperty("media")]
        public ModMediaObject Media { get; }

        [JsonProperty("modfile")]
        public ModfileObject Modfile { get; }

        [JsonProperty("stats")]
        public ModStatsObject Stats { get; }

        [JsonProperty("platforms")]
        public IReadOnlyList<ModPlatformsObject> Platforms { get; }

        [JsonProperty("metadata_kvp")]
        public IReadOnlyList<MetadataKVPObject> MetadataKvp { get; }

        [JsonProperty("tags")]
        public IReadOnlyList<ModTagObject> Tags { get; }

        [JsonConstructor]
        public ModObject(long id, long gameId, int status, int visible, UserObject submittedBy, long dateAdded, long dateUpdated, long dateLive, int maturityOption, int communityOptions, int monetisationOptions, int price, int tax, LogoObject logo, string homepageUrl, string name, string nameId, string summary, string description, string descriptionPlaintext, string metadataBlob, string profileUrl, ModMediaObject media, ModfileObject modfile, ModStatsObject stats, List<ModPlatformsObject> platforms, List<MetadataKVPObject> metadataKvp, List<ModTagObject> tags)
        {
            Id = id;
            GameId = gameId;
            Status = status;
            Visible = visible;
            SubmittedBy = submittedBy;
            DateAdded = dateAdded;
            DateUpdated = dateUpdated;
            DateLive = dateLive;
            MaturityOption = maturityOption;
            CommunityOptions = communityOptions;
            MonetisationOptions = monetisationOptions;
            Price = price;
            Tax = tax;
            Logo = logo;
            HomepageUrl = homepageUrl;
            Name = name;
            NameId = nameId;
            Summary = summary;
            Description = description;
            DescriptionPlaintext = descriptionPlaintext;
            MetadataBlob = metadataBlob;
            ProfileUrl = profileUrl;
            Media = media;
            Modfile = modfile;
            Stats = stats;
            Platforms = platforms;
            MetadataKvp = metadataKvp;
            Tags = tags;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
