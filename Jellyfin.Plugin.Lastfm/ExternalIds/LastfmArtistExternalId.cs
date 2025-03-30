using System.Linq;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Lastfm.ExternalIds;


/// <summary>
/// LastFM artist external id.
/// </summary>
public class LastfmArtistExternalId : IExternalId
{
    /// <summary>
    /// The key.
    /// </summary>
    public const string ProviderKey = "LastFMArtist";

    /// <inheritdoc />
    public string ProviderName => "LastFM";

    /// <inheritdoc />
    public string Key => ProviderKey;

    /// <inheritdoc />
    public ExternalIdMediaType? Type => ExternalIdMediaType.Artist;

    /// <inheritdoc />
    public string UrlFormatString => "https://www.last.fm/music/{0}";

    /// <inheritdoc />
    public bool Supports(IHasProviderIds item) => item is MusicArtist;
    
}