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
    }
}
