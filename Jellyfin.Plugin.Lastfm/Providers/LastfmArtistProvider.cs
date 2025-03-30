using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.Lastfm.Api;
using Jellyfin.Plugin.Lastfm.ExternalIds;
using Jellyfin.Plugin.Lastfm.Models;
using Jellyfin.Plugin.Lastfm.Models.Responses;
using Jellyfin.Plugin.Lastfm.Utils;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Lastfm.Providers
{
    public class LastfmArtistProvider : IRemoteMetadataProvider<MusicArtist, ArtistInfo>, IHasOrder
    {
        
        private readonly IHttpClientFactory _httpClientFactory;
        
        private readonly IServerConfigurationManager _config;
        private readonly ILogger<LastfmArtistProvider> _logger;
        
        private LastfmApiClient _apiClient;

        public LastfmArtistProvider(IHttpClientFactory httpClientFactory, IServerConfigurationManager config, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = loggerFactory.CreateLogger<LastfmArtistProvider>();
            _apiClient = new LastfmApiClient(httpClientFactory, _logger);
        }
        
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
        {
            var musicBrainzId = searchInfo.GetMusicBrainzArtistId();
            var lastfmId = searchInfo.GetLastfmArtistId();
           
            _logger.LogInformation("GetSearchResults: artist: {0}, MBID: {1}, LastFM ID: {2}", searchInfo.Name,musicBrainzId, lastfmId);
            if (!string.IsNullOrWhiteSpace(lastfmId))
            {
                _logger.LogInformation("GetSearchResults: search by LastFM ID: {0} or MusicBrains ID: {1}", lastfmId, musicBrainzId);
                var response = await _apiClient.GetArtistInfo(cancellationToken,musicBrainzId, lastfmId, _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);
                if (!response.IsError() && response.HasArtist())
                {
                    _logger.LogWarning("GetSearchResults: FOUND by LastFM/MusicBrainz id! mbidInResult={0} lastfmId={1}", response.Artist.MusicBrainzId, lastfmId);
                    return [await GetResultFromArtistInfo(response.Artist, cancellationToken)];
                }
                _logger.LogWarning("GetSearchResults: NOT FOUND by LastFM id! mbid={0} lastfmId={1} Error={2}", musicBrainzId, lastfmId, response.Message);
            }
            
            if (!string.IsNullOrWhiteSpace(searchInfo.Name))
            {
                _logger.LogInformation("GetSearchResults: search by artist name: {0}", searchInfo.Name);
                var response = await _apiClient.ArtistSearch(cancellationToken, searchInfo.Name).ConfigureAwait(false);
                if (!response.IsError() && response.HasResults())
                {
                    _logger.LogWarning("GetSearchResults: FOUND by artist name! totalResults={0}", response.Results.TotalResults);
                    return await GetResulstFromSearchResponse(response.Results.ArtistMatches.Artists, cancellationToken);
                }
                _logger.LogWarning("GetSearchResults: NOT FOUND by artist name! mbid={0} lastfmId={1} Error={2}, response={3}", musicBrainzId, lastfmId, response.Message, JsonSerializer.Serialize(response));
            }
            
            return [];
        }

        private async Task<RemoteSearchResult> GetResultFromArtistInfo(Artist artist, CancellationToken cancellationToken)
        {
            var imageUrl = await GetImageInfoAsync(artist.Url, cancellationToken);
            
            if (!string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogInformation("GetSearchResults: Successfully parsed image url: {0}", imageUrl);
            }


            
            var searchResult = new RemoteSearchResult
            {
                Name = artist.Name,
                ImageUrl = imageUrl,
                SearchProviderName = Name,
            };

            if (!string.IsNullOrEmpty(artist.MusicBrainzId))
            {
                searchResult.SetProviderId(MetadataProvider.MusicBrainzArtist, artist.MusicBrainzId);
            }
            
            const string baseUrl = "https://www.last.fm/music/";
            if (!string.IsNullOrEmpty(artist.Url) && artist.Url.StartsWith(baseUrl))
            {
                string artistPart = WebUtility.UrlDecode(artist.Url[baseUrl.Length..].TrimEnd('/'));
                _logger.LogInformation("Try to set LastFM id to {0}", artistPart);
                if (!string.IsNullOrEmpty(artistPart))
                {
                    searchResult.SetProviderId(LastfmArtistExternalId.ProviderKey, artistPart);
                }
            }
            
            return searchResult;
        }

        
        private async Task<IEnumerable<RemoteSearchResult>> GetResulstFromSearchResponse(List<Artist>  matches, CancellationToken cancellationToken)
        {
            var tasks = matches.Select(async match => await GetResultFromArtistInfo(match, cancellationToken));
            return await Task.WhenAll(tasks);
        }


        public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo id, CancellationToken cancellationToken)
        {
         
            var result = new MetadataResult<MusicArtist>();

            var musicBrainzId = id.GetMusicBrainzArtistId();
                
            
            _logger.LogInformation("ArtistGetMetadata mbid={0} name={1} prefferedLanguage={2}", musicBrainzId, id.Name, _config.Configuration.PreferredMetadataLanguage);
            
            cancellationToken.ThrowIfCancellationRequested();
            var response = await _apiClient.GetArtistInfo(cancellationToken,musicBrainzId, id.Name, _config.Configuration.PreferredMetadataLanguage).ConfigureAwait(false);
            if (response.IsError())
            {
                _logger.LogWarning("ArtistGetMetadata mbid={0} name={1} Error={2}", musicBrainzId, id.Name, response.Message);
                return result;
            }
            
            var artist = new MusicArtist();
            
            _logger.LogInformation("ArtistGetMetadata:  received Artist={0}", response.Artist.Name);
            ProcessArtistData(artist, response.Artist);
            
            await LoadImages(musicBrainzId, response.Artist.Url, cancellationToken);
            
            result.Item = artist; 
            result.HasMetadata = true;
            
            return result;
        }

        private async Task LoadImages(string musicBrainzId, string artistUrl, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetImages for artist, ID:{0} Artist URL: {1}", musicBrainzId, artistUrl);
            var imageUrl = await GetImageInfoAsync(artistUrl, cancellationToken);

            if (imageUrl != null)
            {
                _logger.LogInformation("Successfully parsed image url: {0}", imageUrl);
            }

            try
            {
                LastfmHelper.SaveImageInfo(_config.ApplicationPaths, musicBrainzId, imageUrl, "medium");
                _logger.LogInformation("GetImageResponse: saveImageInfo size={0} path={1}", "medium", _config.ApplicationPaths.CachePath);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to save artist image information {0}", e);
            }


            return;
        }
        
        public async Task<string> GetImageInfoAsync(string artistUrl, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetImageInfoAsync: artistUrl: {0}", artistUrl);
            var html = await GetHtmlContentAsync(artistUrl, cancellationToken);
            return ParseImageInfoFromHtml(html);
        }
        
        private string ParseImageInfoFromHtml(string html)
        {
            var imageRegex = new Regex(@"<meta\s+property=""og:image""\s+content=""([^""]+)""", RegexOptions.IgnoreCase);
            var imageMatch = imageRegex.Match(html);
            return !imageMatch.Success ? null : imageMatch.Groups[1].Value;
        }
        
        private async Task<string> GetHtmlContentAsync(string url, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private void ProcessArtistData(MusicArtist artist, Jellyfin.Plugin.Lastfm.Models.Artist data)
        {
            var yearFormed = 0;

            if (data.Bio != null)
            {
                Int32.TryParse(data.Bio.YearFormed, out yearFormed);
                if (!artist.LockedFields.Contains(MetadataField.Overview))
                {
                    artist.Overview = (data.Bio.Content ?? string.Empty).StripHtml();
                }
                if (!string.IsNullOrEmpty(data.Bio.PlaceFormed) && !artist.LockedFields.Contains(MetadataField.ProductionLocations))
                {
                    artist.ProductionLocations = new[] { data.Bio.PlaceFormed };
                }
            }

            if (yearFormed > 0)
            {
                artist.PremiereDate = new DateTime(yearFormed, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                artist.ProductionYear = yearFormed;
            }

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
            _logger.LogInformation("ArtistGetImageResponse: url={0}", url);
            throw new NotImplementedException();
        }
    }
}
