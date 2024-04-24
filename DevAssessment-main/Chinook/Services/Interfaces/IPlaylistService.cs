namespace Chinook.Services.Interfaces
{
    public interface IPlaylistService
    {
        public Task<ClientModels.Playlist> GetPlaylist(long playlistId, string currentUserId);
        public Task<bool> AddTrackToFavoritePlaylist(long trackId, string currentUserId)
;
    }
}
