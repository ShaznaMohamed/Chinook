using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services.Interfaces
{
    public interface IArtistService
    {
        public Task<List<Artist>> GetArtists();
        public Task<List<Album>> GetAlbumsForArtist(int artistId);
        public Task<List<PlaylistTrack>> GetTracksForArtist(long artistId, string currentUserId);
        public Task<Artist> GetSelectedArtist(long artistId);
    }
}
