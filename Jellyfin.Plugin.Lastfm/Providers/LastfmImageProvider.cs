using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Lastfm.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Lastfm.Providers
{
    public class LastfmImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServerConfigurationManager _config;
        private readonly ILogger<LastfmArtistProvider> _logger;


        public LastfmImageProvider(IHttpClientFactory httpClientFactory, IServerConfigurationManager config, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = loggerFactory.CreateLogger<LastfmArtistProvider>();
        }

        public string Name
        {
            get { return ProviderName; }
        }

        public static string ProviderName => "last.fm";

        /// <summary>
        /// Support for Album art only. Last.FM removed support for artist image fetching.
        /// https://github.com/jesseward/jellyfin-plugin-lastfm/issues/25
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Supports(BaseItem item)
        {
            return item is MusicAlbum || item is MusicArtist;
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Logo,
            };
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            RemoteImageInfo info = null;

            if (item is MusicAlbum)
            {
                _logger.LogInformation("GetImages for album, {0}", ((MusicAlbum)item).Name);
                string musicBrainzId = item.GetProviderId(MetadataProvider.MusicBrainzAlbum);
        
                if (!string.IsNullOrEmpty(musicBrainzId))
                {
                    var cachePath = Path.Combine(_config.ApplicationPaths.CachePath, "lastfm", musicBrainzId,
                        "image.txt");

                    try
                    {
                        var parts = File.ReadAllText(cachePath).Split('|');
                        info = GetInfo(parts.FirstOrDefault(), parts.LastOrDefault());
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }

                if (info == null)
                {
                    var musicBrainzReleaseGroupId = item.GetProviderId(MetadataProvider.MusicBrainzReleaseGroup);

                    if (!string.IsNullOrEmpty(musicBrainzReleaseGroupId))
                    {
                        var cachePath = Path.Combine(_config.ApplicationPaths.CachePath, "lastfm",
                            musicBrainzReleaseGroupId, "image.txt");

                        try
                        {
                            var parts = File.ReadAllText(cachePath).Split('|');

                            info = GetInfo(parts.FirstOrDefault(), parts.LastOrDefault());
                        }
                        catch (DirectoryNotFoundException)
                        {
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }

                if (info != null)
                {
                    list.Add(info);
                }
            }
            
            if (item is MusicArtist)
            {
                _logger.LogInformation("GetImages for artist, {0}", ((MusicArtist)item).Name);
                string musicBrainzId = item.GetProviderId(MetadataProvider.MusicBrainzArtist);
                if (!string.IsNullOrEmpty(musicBrainzId))
                {
                    var cachePath = Path.Combine(_config.ApplicationPaths.CachePath, "lastfm", musicBrainzId,
                        "image.txt");

                    try
                    {
                        var parts = File.ReadAllText(cachePath).Split('|');

                        info = GetInfo(parts.FirstOrDefault(), parts.LastOrDefault());
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }

            }
            
            

            // The only info we have is size
            return Task.FromResult<IEnumerable<RemoteImageInfo>>(list.OrderByDescending(i => i.Width ?? 0));
        }

        private RemoteImageInfo GetInfo(string url, string size)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var info = new RemoteImageInfo
            {
                ProviderName = Name,
                Url = url,
                Type = ImageType.Primary
            };

            if (string.Equals(size, "mega", StringComparison.OrdinalIgnoreCase))
            {
                
            }
            else if (string.Equals(size, "extralarge", StringComparison.OrdinalIgnoreCase))
            {

            }
            else if (string.Equals(size, "large", StringComparison.OrdinalIgnoreCase))
            {

            }
            else if (string.Equals(size, "medium", StringComparison.OrdinalIgnoreCase))
            {

            }

            return info;
        }

        public int Order
        {
            get
            {
                // After all others
                return 3;
            }
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetImageResponse: url={0}", url);
            return _httpClientFactory.CreateClient().GetAsync(url, cancellationToken);
        }
    }
}
