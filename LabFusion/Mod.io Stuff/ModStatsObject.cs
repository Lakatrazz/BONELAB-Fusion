using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModStatsObject
    {
        [JsonProperty("mod_id")]
        public long ModId { get; }

        [JsonProperty("popularity_rank_position")]
        public int PopularityRankPosition { get; }

        [JsonProperty("popularity_rank_total_mods")]
        public int PopularityRankTotalMods { get; }

        [JsonProperty("downloads_today")]
        public int DownloadsToday { get; }

        [JsonProperty("downloads_total")]
        public int DownloadsTotal { get; }

        [JsonProperty("downloads_daily_average")]
        public float DownloadsDailyAverage { get; }

        [JsonProperty("subscribers_total")]
        public int SubscribersTotal { get; }

        [JsonProperty("ratings_total")]
        public int RatingsTotal { get; }

        [JsonProperty("ratings_positive")]
        public int RatingsPositive { get; }

        [JsonProperty("ratings_negative")]
        public int RatingsNegative { get; }

        [JsonProperty("ratings_percentage_positive")]
        public int RatingsPercentagePositive { get; }

        [JsonProperty("ratings_weighted_aggregate")]
        public float RatingsWeightedAggregate { get; }

        [JsonProperty("ratings_display_text")]
        public string RatingsDisplayText { get; }

        [JsonProperty("date_expires")]
        public long DateExpires { get; }

        [JsonConstructor]
        public ModStatsObject(long modId, int popularityRankPosition, int popularityRankTotalMods, int downloadsToday, int downloadsTotal, float downloadsDailyAverage, int subscribersTotal, int ratingsTotal, int ratingsPositive, int ratingsNegative, int ratingsPercentagePositive, float ratingsWeightedAggregate, string ratingsDisplayText, long dateExpires)
        {
            ModId = modId;
            PopularityRankPosition = popularityRankPosition;
            PopularityRankTotalMods = popularityRankTotalMods;
            DownloadsToday = downloadsToday;
            DownloadsTotal = downloadsTotal;
            DownloadsDailyAverage = downloadsDailyAverage;
            SubscribersTotal = subscribersTotal;
            RatingsTotal = ratingsTotal;
            RatingsPositive = ratingsPositive;
            RatingsNegative = ratingsNegative;
            RatingsPercentagePositive = ratingsPercentagePositive;
            RatingsWeightedAggregate = ratingsWeightedAggregate;
            RatingsDisplayText = ratingsDisplayText;
            DateExpires = dateExpires;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
