namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class ArtistSearchResults
    {
        [JsonPropertyName("opensearch:Query")]
        public OpenSearchQuery Query { get; set; }

        [JsonPropertyName("opensearch:totalResults")]
        public string TotalResults { get; set; }

        [JsonPropertyName("opensearch:startIndex")]
        public string StartIndex { get; set; }

        [JsonPropertyName("opensearch:itemsPerPage")]
        public string ItemsPerPage { get; set; }

        [JsonPropertyName("artistmatches")]
        public ArtistMatches ArtistMatches { get; set; }

        [JsonPropertyName("@attr")]
        public SearchAttributes Attributes { get; set; }
    }

    public class OpenSearchQuery
    {
        [JsonPropertyName("#text")]
        public string Text { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("searchTerms")]
        public string SearchTerms { get; set; }

        [JsonPropertyName("startPage")]
        public string StartPage { get; set; }
    }

    public class ArtistMatches
    {
        [JsonPropertyName("artist")]
        public List<Artist> Artists { get; set; }
    }

    public class SearchAttributes
    {
        [JsonPropertyName("for")]
        public string For { get; set; }
    }
}