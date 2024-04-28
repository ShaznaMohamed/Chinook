using AutoMapper;
using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class PlaylistService(IDbContextFactory<ChinookContext> _dbFactory, IMapper _mapper) : IPlaylistService
    {
        private static readonly string MY_FAVORITE_PLAYLIST_NAME = "My favorite tracks";

        #region Public Methods

        // Set a playlist as favorite 
        public async Task<bool> AddTrackToFavoritePlaylist(long trackId, string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();

            long favoritePlaylistId = 100000;  // Added random long value for unique playlist id for the 'My favorite play list'
            var myFavoritePlaylist = new Chinook.Models.Playlist()      // Create a new playlist object named 'My favorite tracks'
            {
                PlaylistId = favoritePlaylistId,
                Name = MY_FAVORITE_PLAYLIST_NAME
            };

            // Check whether a playlist named 'My favorite tracks' already exists, if not add to the plsylist
            SetMyFavoriteTrackToPlaylist(dbContext, favoritePlaylistId, myFavoritePlaylist);

            // Add the selected track in to 'My favorite track' if it is not found already
            SetMyFavoriteTrackToTracks(dbContext, favoritePlaylistId, myFavoritePlaylist, trackId);

            // Check whether current user has any userPlayList if not add the 'My favorite track' to current user's userplaylist
            SetMyFavoriteTrackToUserPlaylist(dbContext, currentUserId, favoritePlaylistId, myFavoritePlaylist);

            return await dbContext.SaveChangesAsync() > 0;      // Save playlist as favorite in db

        }

        // Unfavorite the selected track
        public async Task<bool> RemoveTrackFromMyFavoritePlaylist(long trackId, string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            var userPlaylist = dbContext.UserPlaylists.Include(p => p.Playlist).FirstOrDefault(u => u.UserId == currentUserId);  // Get the user playlist

            if (userPlaylist != null)
            {
                long favoritePlaylistId = userPlaylist.PlaylistId;      // Get the user's favorite playlist id
                var selectedTrack = dbContext.Tracks.Include(p => p.Playlists).FirstOrDefault(t => t.TrackId == trackId);   // Get the selected track
                if (selectedTrack != null)
                {
                    var unFavoritePlaylist = selectedTrack.Playlists.FirstOrDefault(p => p.PlaylistId == favoritePlaylistId);  // Check whether the selected track's playlist is favorite
                    if (unFavoritePlaylist != null)
                    {
                        selectedTrack.Playlists.Remove(unFavoritePlaylist);   // Remove the favorite playlist and make it unfavorite
                        return await dbContext.SaveChangesAsync() > 0;        // Save unfavorite  change in db
                    }
                }
            }
            return false;
        }

        // Get the current user's favorite playlist id
        public async Task<long> GetCurrentUserFavoritePlaylistId(string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            var userPlaylist = dbContext.UserPlaylists.Include(p => p.Playlist)  // Get the user's favorite playlist
                                        .Where(up => up.UserId == currentUserId && up.Playlist != null && up.Playlist.Name == MY_FAVORITE_PLAYLIST_NAME)
                                        .FirstOrDefault();
            return userPlaylist != null ? userPlaylist.PlaylistId : 0;   // 0 is set as default playlist id when current user does not have any favorite playlists
        }

        // Get the selected playlist details
        public async Task<ClientModels.Playlist> GetPlaylist(long playlistId, string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            var selectedPlaylist = dbContext.Playlists.Include(a => a.UserPlaylists).Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                                .Where(p => p.PlaylistId == playlistId).FirstOrDefault();    // Retrieves the selected playlist
            ClientModels.Playlist response = new ClientModels.Playlist();
            if (selectedPlaylist != null)
            {
                response.Name = selectedPlaylist.Name ?? string.Empty;
                response.Tracks = new List<PlaylistTrack>();
                if (selectedPlaylist.Tracks != null)
                {
                    selectedPlaylist.Tracks.ToList().ForEach(track =>
                    {
                        response.Tracks.Add(MapToPlaylistTrack(track, currentUserId));     // Map the Track model to PlaylistTrack model from Auto Mapper
                    });
                }
            }
            return response;
        }

        #endregion Public Methods


        #region Private Methods

        // Map the Track model to PlaylistTrack model and set the IsFavorite attribute value AfterMap 
        private PlaylistTrack MapToPlaylistTrack(Track track, string currentUserId)
        {
            return _mapper.Map<Track, PlaylistTrack>(track, opt =>
                                opt.AfterMap((src, dest) =>                    // AfterMap set the IsFavorite value based on currentUserId
                                    dest.IsFavorite = src.Playlists
                                                      .Where(p => p.UserPlaylists != null &&
                                                      p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist != null && up.Playlist.Name == MY_FAVORITE_PLAYLIST_NAME)).Any()));
        }

        // Add 'My favorite tracks' to UserPlaylist 
        private void SetMyFavoriteTrackToUserPlaylist(ChinookContext dbContext, string currentUserId, long PlayListId, Models.Playlist playlist)
        {
            var currentUser = dbContext.Users.FirstOrDefault(u => u.Id == currentUserId);
            var currentUserPlayList = dbContext.UserPlaylists.Include(p => p.Playlist).Any(u => u.UserId == currentUserId);
            if (!currentUserPlayList)
            {
                UserPlaylist newUserPlaylist = new UserPlaylist()
                {
                    UserId = currentUserId,
                    User = currentUser,
                    Playlist = playlist,
                    PlaylistId = PlayListId
                };
                dbContext.UserPlaylists.Add(newUserPlaylist);
            }
        }

        // Add 'My favorite tracks' to Playlist 
        private void SetMyFavoriteTrackToPlaylist(ChinookContext dbContext, long playListId, Models.Playlist playlist)
        {
            var favoritePlaylist = dbContext.Playlists.Any(p => p.Name == MY_FAVORITE_PLAYLIST_NAME && p.PlaylistId == playListId);
            if (!favoritePlaylist)
            {
                dbContext.Playlists.Add(playlist);   // Add 'My favorite track' playlist to the playlists
            }
        }

        // Add 'My favorite tracks' to Tracks 
        private void SetMyFavoriteTrackToTracks(ChinookContext dbContext, long playListId, Models.Playlist playlist, long trackId)
        {
            var selectedTrack = dbContext.Tracks.Include(p => p.Playlists).FirstOrDefault(t => t.TrackId == trackId);       // Get the selected track
            if (selectedTrack != null && !selectedTrack.Playlists.Any(p => p.PlaylistId == playListId && p.Name == MY_FAVORITE_PLAYLIST_NAME))
            {
                selectedTrack.Playlists.Add(playlist);
                dbContext.Playlists.Attach(playlist);
            }
        }

        #endregion Private Methods
    }
}
