namespace Jellyfin.Plugin.Lastfm.Models.Requests
{
    using System.Collections.Generic;

    public class GetArtistInfoRequest : BaseRequest
    {
        /// <summary>
        /// Artist name to match
        /// </summary>
        public string Artist { get; set; }
        
        /// <summary>
        /// MusicBrains ID to match
        /// </summary>
        public string MBID { get; set; }
        
        /// <summary>
        /// Language code
        /// </summary>
        public string Lang { get; set; }
        
        /// <summary>
        /// Autocorrect misspelled names
        /// </summary>
        public bool Autocorrect { get; set; }
        
        /// <summary>
        /// Username to return plays count
        /// </summary>
        public string User { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var request = new Dictionary<string, string>(base.ToDictionary()) 
            {
                { "autocorrect", Autocorrect ? "1" : "0" },
            };
            
            if (!string.IsNullOrWhiteSpace(Artist))
            {
                request.Add("artist", Artist);
            }
            
            if (!string.IsNullOrWhiteSpace(User))
            {
                request.Add("username", User);
            }
            
            if (!string.IsNullOrWhiteSpace(Lang))
            {
                request.Add("lang", Lang);
            }
            
            if (!string.IsNullOrWhiteSpace(MBID))
            {
                request.Add("mbid", MBID);
            }
            
            return request;
            
        }
    }
}