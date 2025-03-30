using System.Collections.Generic;
using System.Net;
using Jellyfin.Plugin.Lastfm.ExternalIds;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Lastfm.Providers;

/// <summary>
/// External artist URLs for LastFM.
/// </summary>
public class LastfmArtistExternalUrlProvider : IExternalUrlProvider
{
    /// <inheritdoc/>
    public string Name => "LastFM Artist";

    /// <inheritdoc/>
    public IEnumerable<string> GetExternalUrls(BaseItem item)
    {
        if (item.TryGetProviderId(LastfmArtistExternalId.ProviderKey, out var externalId))
        {
            switch (item)
            {
                case MusicAlbum:
                case Person:
                    yield return  $"https://www.lastfm.com/music/{WebUtility.UrlEncode(externalId)}";

                    break;
            }
        }
    }
}