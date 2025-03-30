namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

 public class Album : IHasImages
    {
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("listeners")]
        public string Listeners { get; set; }

        [JsonPropertyName("image")]
        public List<Image> Images { get; set; }

        [JsonPropertyName("tracks")]
        public AlbumTracks Tracks { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }

        [JsonPropertyName("wiki")]
        public Bio Wiki { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("playcount")]
        public string PlayCount { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
        
        [JsonPropertyName("releasedate")]
        public string ReleaseDate { get; set; }
    }

    public class AlbumTracks
    {
        [JsonPropertyName("track")]
        public List<Track> TrackList { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("streamable")]
        public Streamable Streamable { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("@attr")]
        public TrackAttribute Attributes { get; set; }

        [JsonPropertyName("artist")]
        public TrackArtist Artist { get; set; }
    }

    public class Streamable
    {
        [JsonPropertyName("fulltrack")]
        public string FullTrack { get; set; }

        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

    public class TrackAttribute
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }
    }

    public class TrackArtist
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string MusicBrainzId { get; set; }
    }
}