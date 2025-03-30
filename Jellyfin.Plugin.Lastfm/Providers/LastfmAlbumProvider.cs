using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Lastfm.Api;
using Jellyfin.Plugin.Lastfm.Models;
using Jellyfin.Plugin.Lastfm.Models.Responses;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Lastfm.Providers
{
    public class LastfmAlbumProvider : IRemoteMetadataProvider<MusicAlbum, AlbumInfo>, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IServerConfigurationManager _config;
        private readonly ILogger<LastfmAlbumProvider> _logger;
        private LastfmApiClient _apiClient;
        

        public LastfmAlbumProvider(IHttpClientFactory httpClientFactory, IServerConfigurationManager config, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = loggerFactory.CreateLogger<LastfmAlbumProvider>();
            _apiClient = new LastfmApiClient(httpClientFactory, _logger);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(AlbumInfo searchInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }

        public async Task<MetadataResult<MusicAlbum>> GetMetadata(AlbumInfo id, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<MusicAlbum>();

            var musicBrainzId = id.GetProviderId(MetadataProvider.MusicBrainzAlbum) ??
                id.GetProviderId(MetadataProvider.MusicBrainzReleaseGroup);
            
            _logger.LogInformation($"GetMetadata: Getting album {musicBrainzId}");

            var lastFmData = await GetAlbumResult(id, cancellationToken).ConfigureAwait(false);
            
            if (lastFmData != null && lastFmData.HasAlbum())
            {
                _logger.LogInformation($"GetMetadata: Has metadata for album {musicBrainzId}");
                _logger.LogInformation($"AlbumData: {JsonSerializer.Serialize(lastFmData.Album)}");
                result.HasMetadata = true;
                result.Item = new MusicAlbum();
                ProcessAlbumData(result.Item, lastFmData.Album, musicBrainzId);
            }

            return result;
        }

        private async Task<GetAlbumInfoResponse> GetAlbumResult(AlbumInfo item, CancellationToken cancellationToken)
        {
            // Try album release Id
            var id = item.GetReleaseId();
            if (!string.IsNullOrEmpty(id))
            {
                var result = await _apiClient.GetAlbumInfo(cancellationToken,id, "", "", _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);

                if (result != null && result.HasAlbum())
                {
                    _logger.LogInformation("GetAlbumResult: Found by release id [{0}]: {1}/{2}",  id, result.Album.Artist, result.Album.Name);
                    return result;
                }
            }

            // Try album release group Id
            id = item.GetReleaseGroupId();
            if (!string.IsNullOrEmpty(id))
            {
                var result = await _apiClient.GetAlbumInfo(cancellationToken,id, "", "", _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);

                if (result != null && result.HasAlbum())
                {
                    _logger.LogInformation("GetAlbumResult: Found by release group id [{0}]: {1}/{2}",  id, result.Album.Artist, result.Album.Name);
                    return result;
                }
            }

            var albumArtist = item.GetAlbumArtist();
            // Get each song, distinct by the combination of AlbumArtist and Album
            var songs = item.SongInfos.DistinctBy(i => (i.AlbumArtists.FirstOrDefault() ?? string.Empty) + (i.Album ?? string.Empty), StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var song in songs.Where(song => !string.IsNullOrEmpty(song.Album) && !string.IsNullOrEmpty(song.AlbumArtists.FirstOrDefault())))
            {
                var result = await _apiClient.GetAlbumInfo(cancellationToken,"", song.AlbumArtists.FirstOrDefault(), song.Album, _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);
                
                if (result != null && result.HasAlbum())
                {
                    _logger.LogInformation("GetAlbumResult: Found by songs  [{0}]: {1}/{2}",  id, result.Album.Artist, result.Album.Name);
                    return result;
                }
            }

            if (string.IsNullOrEmpty(albumArtist))
            {
                return null;
            }
            
            return  await _apiClient.GetAlbumInfo(cancellationToken,"", albumArtist, item.Name, _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);
        }

        private void ProcessAlbumData(MusicAlbum item, Album data, string musicBrainzId)
        {
            _logger.LogInformation("ProcessAlbumData: id={0}, musicAlbum.id={1}, lastfmName={2}", musicBrainzId, item.Id, data.Name);
            var overview = (data.Wiki != null) ? data.Wiki.Content : null;

            if (!item.LockedFields.Contains(MetadataField.Overview))
            {
                item.Overview = overview;
            }

            // Only grab the date here if the album doesn't already have one, since id3 tags are preferred
            DateTime release;

            if (DateTime.TryParse(data.ReleaseDate, out release))
            {
                // Lastfm sends back null as sometimes 1901, other times 0
                if (release.Year > 1901)
                {
                    if (!item.PremiereDate.HasValue)
                    {
                        item.PremiereDate = release;
                    }

                    if (!item.ProductionYear.HasValue)
                    {
                        item.ProductionYear = release.Year;
                    }
                }
            }

            var url = LastfmHelper.GetImageUrl(data, out string imageSize);
            _logger.LogInformation("GetImageResponse: image url={0}", url);
            if (!string.IsNullOrEmpty(musicBrainzId) && !string.IsNullOrEmpty(url))
            {
                try
                {
                    LastfmHelper.SaveImageInfo(_config.ApplicationPaths, musicBrainzId, url, imageSize);
                    _logger.LogInformation("GetImageResponse: saveImageInfo size={0} path={1}", imageSize, _config.ApplicationPaths.CachePath);
                }
                catch (Exception e)
                {
                    _logger.LogError("Failed to save image information {0}", e);
                }

            }
        }

        /// <summary>
        /// Encodes an URL.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private string UrlEncode(string name)
        {
            return WebUtility.UrlEncode(name);
        }

        public string Name
        {
            get { return "last.fm"; }
        }

        public int Order
        {
            get
            {
                // After fanart & audiodb
                return 2;
            }
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlbumGetImageResponse: url={0}", url);
            throw new NotImplementedException();
        }
    }

    #region Result Objects

    public class LastfmStats
    {
        public string listeners { get; set; }
        public string playcount { get; set; }
    }

    public class LastfmTag
    {
        public string name { get; set; }
        public string url { get; set; }
    }


    public class LastfmTags
    {
        public List<LastfmTag> tag { get; set; }
    }

    public class LastfmFormationInfo
    {
        public string yearfrom { get; set; }
        public string yearto { get; set; }
    }

    public class LastFmBio
    {
        public string published { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
        public string placeformed { get; set; }
        public string yearformed { get; set; }
        public List<LastfmFormationInfo> formationlist { get; set; }
    }

    public class LastFmImage
    {
        public string url { get; set; }
        public string size { get; set; }
    }

    // public class LastfmArtist : IHasLastFmImages
    // {
    //     public string name { get; set; }
    //     public string mbid { get; set; }
    //     public string url { get; set; }
    //     public string streamable { get; set; }
    //     public string ontour { get; set; }
    //     public LastfmStats stats { get; set; }
    //     public List<LastfmArtist> similar { get; set; }
    //     public LastfmTags tags { get; set; }
    //     public LastFmBio bio { get; set; }
    //     public List<LastFmImage> image { get; set; }
    // }


    // public class LastfmAlbum : IHasLastFmImages
    // {
    //     public string name { get; set; }
    //     public string artist { get; set; }
    //     public string id { get; set; }
    //     public string mbid { get; set; }
    //     public string releasedate { get; set; }
    //     public int listeners { get; set; }
    //     public int playcount { get; set; }
    //     public LastfmTags toptags { get; set; }
    //     public LastFmBio wiki { get; set; }
    //     public List<LastFmImage> image { get; set; }
    // }
    //
    // public interface IHasLastFmImages
    // {
    //     List<LastFmImage> image { get; set; }
    // }
    //
    // public class LastfmGetAlbumResult
    // {
    //     public LastfmAlbum album { get; set; }
    // }
    //
    // public class LastfmGetArtistResult
    // {
    //     public LastfmArtist artist { get; set; }
    // }
    //
    // public class Artistmatches
    // {
    //     public List<LastfmArtist> artist { get; set; }
    // }

    // public class LastfmArtistSearchResult
    // {
    //     public Artistmatches artistmatches { get; set; }
    // }

    // public class LastfmArtistSearchResults
    // {
    //     public LastfmArtistSearchResult results { get; set; }
    // }

    #endregion
}
