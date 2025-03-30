﻿namespace Jellyfin.Plugin.Lastfm.Resources
{
    public static class Strings
    {
        public static class Endpoints
        {
            public static string LastfmApi  = "ws.audioscrobbler.com";
        }

        public static class Methods
        {
            // Last.FM API specs located at https://www.last.fm/api
            public static string Scrobble         = "track.scrobble";
            public static string NowPlaying       = "track.updateNowPlaying";
            public static string GetMobileSession = "auth.getMobileSession";
            public static string TrackLove        = "track.love";
            public static string TrackUnlove      = "track.unlove";
            public static string GetLovedTracks   = "user.getLovedTracks";
            public static string GetTracks        = "library.getTracks";
            public static string ArtistGetInfo    = "artist.getInfo";
            public static string AlbumGetInfo     = "album.getInfo";
            public static string ArtistSearch     = "artist.search";
        }

        public static class Keys
        {
            public static string LastfmApiKey     = "cb3bdcd415fcb40cd572b137b2b255f5";
            public static string LastfmApiSeceret = "3a08f9fad6ddc4c35b0dce0062cecb5e";
        }
    }
}
