namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    
    public interface IHasImages
    {
        List<Image> Images { get; set; }
    }

    public class Artist: IHasImages
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string MusicBrainzId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("image")]
        public List<Image> Images { get; set; }

        [JsonPropertyName("streamable")]
        [JsonConverter(typeof(JsonStringToBoolConverter))]
        public bool Streamable { get; set; }

        [JsonPropertyName("ontour")]
        [JsonConverter(typeof(JsonStringToBoolConverter))]
        public bool OnTour { get; set; }

        [JsonPropertyName("stats")]
        public Stats Stats { get; set; }

        [JsonPropertyName("similar")]
        public Similar Similar { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }

        [JsonPropertyName("bio")]
        public Bio Bio { get; set; }
    }
    
    public class Image
    {
        [JsonPropertyName("#text")]
        public string Url { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }
    }

    public class Stats
    {
        [JsonPropertyName("listeners")]
        public string Listeners { get; set; }

        [JsonPropertyName("playcount")]
        public string PlayCount { get; set; }
    }

    public class SimilarArtist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("image")]
        public List<Image> Images { get; set; }
    }

    public class Similar
    {
        [JsonPropertyName("artist")]
        public List<SimilarArtist> Artists { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
    
    public class Tags
    {
        [JsonPropertyName("tag")]
        public List<Tag> TagsList { get; set; }
    }

    
    
    public class Link
    {
        [JsonPropertyName("#text")]
        public string Text { get; set; }

        [JsonPropertyName("rel")]
        public string Rel { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Links
    {
        [JsonPropertyName("link")]
        public Link Link { get; set; }
    }

    public class Bio
    {
        [JsonPropertyName("links")]
        public Links Links { get; set; }

        [JsonPropertyName("published")]
        public string Published { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
        
        [JsonPropertyName("placeformed")]
        public string PlaceFormed { get; set; }
        
        [JsonPropertyName("yearformed")]
        public string YearFormed { get; set; }
        
        [JsonPropertyName("formationlist")]
        public List<FormationInfo> FormationList { get; set; }

    }
    
    public class FormationInfo
    {
        [JsonPropertyName("yearfrom")]
        public string YearFrom { get; set; }
        
        [JsonPropertyName("yearto")]
        public string YearTo { get; set; }
    }

    
}
    

