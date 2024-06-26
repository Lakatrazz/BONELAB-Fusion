using System;
using Newtonsoft.Json;

namespace SLZ.ModIO.ApiModels
{
    [Serializable]
    public readonly struct ModfileObject
    {
        [JsonProperty("id")]
        public long Id { get; }

        [JsonProperty("mod_id")]
        public long ModId { get; }

        [JsonProperty("date_added")]
        public long DateAdded { get; }

        [JsonProperty("date_scanned")]
        public long DateScanned { get; }

        [JsonProperty("virus_status")]
        public int VirusStatus { get; }

        [JsonProperty("virus_positive")]
        public int VirusPositive { get; }

        [JsonProperty("virustotal_hash")]
        public string VirusTotalHash { get; }

        [JsonProperty("filesize")]
        public long Filesize { get; }

        [JsonProperty("filehash")]
        public FilehashObject Filehash { get; }

        [JsonProperty("filename")]
        public string Filename { get; }

        [JsonProperty("version")]
        public string Version { get; }

        [JsonProperty("changelog")]
        public string Changelog { get; }

        [JsonProperty("metadata_blob")]
        public string MetadataBlob { get; }

        [JsonProperty("download")]
        public DownloadObject Download { get; }

        [JsonConstructor]
        public ModfileObject(long id, long modId, long dateAdded, long dateScanned, int virusStatus, int virusPositive, string virusTotalHash, long filesize, FilehashObject filehash, string filename, string version, string changelog, string metadataBlob, DownloadObject download)
        {
            Id = id;
            ModId = modId;
            DateAdded = dateAdded;
            DateScanned = dateScanned;
            VirusStatus = virusStatus;
            VirusPositive = virusPositive;
            VirusTotalHash = virusTotalHash;
            Filesize = filesize;
            Filehash = filehash;
            Filename = filename;
            Version = version;
            Changelog = changelog;
            MetadataBlob = metadataBlob;
            Download = download;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
