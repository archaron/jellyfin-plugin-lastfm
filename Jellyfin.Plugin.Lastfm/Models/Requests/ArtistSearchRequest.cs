namespace Jellyfin.Plugin.Lastfm.Models.Requests
{
    using System.Collections.Generic;

    public class ArtistSearchRequest : BaseRequest
    {
        /// <summary>
        /// Artist name to match
        /// </summary>
        public string Artist { get; set; }

        public int    Limit  { get; set; }
        
        public int    Page   { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var request = new Dictionary<string, string>(base.ToDictionary())
            {
                { "artist", Artist },
                { "page", Page.ToString() },
                { "limit", Limit.ToString() },
            };
            return request;
        }
    }
}