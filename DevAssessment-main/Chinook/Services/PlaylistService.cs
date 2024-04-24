using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class PlaylistService(IDbContextFactory<ChinookContext> _dbFactory) : IPlaylistService
    {
        public async Task<ClientModels.Playlist> GetPlaylist(long playlistId, string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            return dbContext.Playlists
            .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == playlistId)
            .Select(p => new ClientModels.Playlist()
            {
                Name = p.Name,
                Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "My favorite tracks")).Any()
                }).ToList()
            })
            .FirstOrDefault();
        }

        public async Task<bool> AddTrackToFavoritePlaylist(long trackId, string currentUserId)
        {
            try
            {
                var dbContext = await _dbFactory.CreateDbContextAsync();
                var currentUser = dbContext.Users.FirstOrDefault(u => u.Id == currentUserId);

                long favoritPlaylistId = 100000;  // Added random long value for unique id

                var myFavoritePlaylist = new Chinook.Models.Playlist()      // Create a new playlist named My favorite tracks
                {
                    PlaylistId = favoritPlaylistId,
                    Name = "My favorite tracks"
                };
                var favoritePlaylist = dbContext.Playlists.Any(p => p.Name == "My favorite tracks" && p.PlaylistId == favoritPlaylistId);      // Check whether a playlist named 'My favorite tracks' already exists
                if (!favoritePlaylist)
                {
                    dbContext.Playlists.Add(myFavoritePlaylist);   // Add my favorite track playlist to the playlists
                }


                var selectedTrack = dbContext.Tracks.Include(p => p.Playlists).FirstOrDefault(t => t.TrackId == trackId);                   // Get the selected track
                if (selectedTrack != null && !selectedTrack.Playlists.Any(p => p.PlaylistId == favoritPlaylistId && p.Name == "My favorite tracks"))    // Add the selected track in to my favorite playlist if it is not found already
                {
                    selectedTrack.Playlists.Add(myFavoritePlaylist);
                    dbContext.Playlists.Attach(myFavoritePlaylist);
                }

                var currentUserPlayList = dbContext.UserPlaylists.Include(p => p.Playlist).Any(u => u.UserId == currentUserId);    // Check whether current user has any userplaylist if not add the my user playlist to current user's userplaylist
                if (!currentUserPlayList)
                {
                    UserPlaylist newUserPlaylist = new UserPlaylist()
                    {
                        UserId = currentUserId,
                        User = currentUser,
                        Playlist = myFavoritePlaylist,
                        PlaylistId = favoritPlaylistId
                    };
                    dbContext.UserPlaylists.Add(newUserPlaylist);
                }
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch(Exception e)
            {
                return false;
            }
            
        }

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
                        return await dbContext.SaveChangesAsync() > 0;
                    }
                }
            }
            return false;
        }
    }
}
