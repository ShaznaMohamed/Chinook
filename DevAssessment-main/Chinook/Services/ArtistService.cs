using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class ArtistService(IDbContextFactory<ChinookContext> _dbFactory) : IArtistService
    {
        public async Task<List<Artist>> GetArtists()
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return dbContext.Artists.AsNoTracking().Include(a => a.Albums).ToList();
        }
        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

        public async Task<List<PlaylistTrack>> GetTracksForArtist(long artistId, string currentUserId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            
            return dbContext.Tracks.Where(a => a.Album != null && a.Album.ArtistId == artistId)
            .Include(a => a.Album)
            .Select(t => new PlaylistTrack()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites")).Any()
            })
            .ToList();
        }

        public async Task<Artist> GetSelectedArtist(long artistId)
        {
            var dbContext = await _dbFactory.CreateDbContextAsync();
            return dbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
        }
    }
}
