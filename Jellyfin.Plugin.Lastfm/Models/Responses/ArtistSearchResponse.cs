namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Text.Json.Serialization;

    public class ArtistSearchResponse : BaseResponse
    {
        [JsonPropertyName("results")] public ArtistSearchResults Results { get; set; }

        public bool HasResults()
        {
            return Results != null;
        }
    }
}