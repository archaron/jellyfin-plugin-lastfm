using Jellyfin.Plugin.Lastfm.Models;

namespace Jellyfin.Plugin.Lastfm.Providers
{
    using MediaBrowser.Common.Configuration;
    using System;
    using System.IO;
    using System.Linq;

    public static class LastfmHelper
    {
        public static string GetImageUrl(IHasImages data, out string size)
        {
            size = null;

            if (data.Images == null)
            {
                return null;
            }

            var validImages = data.Images
                .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                .ToList();

            var img = validImages
                .FirstOrDefault(i => string.Equals(i.Size, "mega", StringComparison.OrdinalIgnoreCase)) ??
                data.Images.FirstOrDefault(i => string.Equals(i.Size, "extralarge", StringComparison.OrdinalIgnoreCase)) ??
                data.Images.FirstOrDefault(i => string.Equals(i.Size, "large", StringComparison.OrdinalIgnoreCase)) ??
                data.Images.FirstOrDefault(i => string.Equals(i.Size, "medium", StringComparison.OrdinalIgnoreCase)) ??
                data.Images.FirstOrDefault();

            if (img != null)
            {
                size = img.Size;
                return img.Url;
            }

            return null;
        }

        /// <exception cref="ArgumentNullException">thrown when string params are null.</exception>
        /// <exception cref="IOException">Unable to perform file operation on cachePath.</exception>
        public static void SaveImageInfo(IApplicationPaths appPaths, string musicBrainzId, string url, string size)
        {
            if (appPaths == null)
            {
                throw new ArgumentNullException("appPaths");
            }
            if (string.IsNullOrEmpty(musicBrainzId))
            {
                throw new ArgumentNullException("musicBrainzId");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var cachePath = Path.Combine(appPaths.CachePath, "lastfm", musicBrainzId, "image.txt");

            if (string.IsNullOrEmpty(url))
            {
                File.Delete(cachePath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                File.WriteAllText(cachePath, url + "|" + size);
            }

        }
    }
}
