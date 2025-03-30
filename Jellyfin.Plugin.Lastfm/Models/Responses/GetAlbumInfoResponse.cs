namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Text.Json.Serialization;

    public class GetAlbumInfoResponse : BaseResponse
    {
        [JsonPropertyName("album")]
        public Album Album { get; set; }

        public bool HasAlbum()
        {
            return Album != null;
        }
    }
}