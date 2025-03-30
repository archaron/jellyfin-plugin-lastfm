namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GetArtistInfoResponse : BaseResponse
    {
        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }

        public bool HasArtist()
        {
            return Artist != null;
        }
    }
}