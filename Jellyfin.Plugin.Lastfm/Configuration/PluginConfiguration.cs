﻿namespace Jellyfin.Plugin.Lastfm.Configuration
{
    using Models;
    using MediaBrowser.Model.Plugins;

    /// <summary>
    /// Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public LastfmUser[] LastfmUsers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            LastfmUsers = new LastfmUser[] { };
            ApiKey = string.Empty;
        }
        
        /// <summary>
        /// Gets or sets a ApiKey setting.
        /// </summary>
        public string ApiKey { get; set; }

    }
}
